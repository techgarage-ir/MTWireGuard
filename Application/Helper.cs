using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Filters;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MTWireGuard.Application
{
    public class Helper
    {
        public static readonly string[] UpperCaseTopics =
        [
            "dhcp",
            "ppp",
            "l2tp",
            "pptp",
            "sstp"
        ];

        public static string PeersTrafficUsageScript(string apiURL)
        {
            return $"/tool fetch mode=http url=\"{apiURL}\" http-method=post check-certificate=no http-data=([/interface/wireguard/peers/print show-ids proplist=rx,tx as-value]);";
        }

        public static string UserExpirationScript(string userID)
        {
            return $"/interface/wireguard/peers/disable {userID}";
        }

        public static int ParseEntityID(string entityID)
        {
            return Convert.ToInt32(entityID[1..], 16);
        }

        public static string ParseEntityID(int entityID)
        {
            return $"*{entityID:X}";
        }

        private static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        public static string ConvertByteSize(long value, int decimalPlaces = 2)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == 0) { return "0"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        public static string ConvertByteSize(ulong value, int decimalPlaces = 2)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == 0) { return "0"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1UL << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static ulong GigabyteToByte(int gigabyte)
        {
            return Convert.ToUInt64(gigabyte * (1024L * 1024 * 1024));
        }

        #region API Section
        public static List<UsageObject> ParseTrafficUsage(string input)
        {
            string[] items = input.Split(".id=");

            List<string> objects = items
                .Where(item => !string.IsNullOrEmpty(item))
                .Select(item => $"id={item}")
                .ToList();

            return objects
                .Select(x => x.Split(';').ToList())
                .Select(arr =>
                {
                    var obj = new UsageObject();
                    var id = arr.Find(x => x.Contains("id")).Split('=')[1];
                    var rx = arr.Find(x => x.Contains("rx")).Split('=')[1] ?? "0";
                    var tx = arr.Find(x => x.Contains("tx")).Split('=')[1] ?? "0";
                    obj.Id = id;
                    obj.RX = ulong.Parse(rx);
                    obj.TX = ulong.Parse(tx);
                    return obj;
                }).ToList();
        }

        public static List<UserActivityUpdate> ParseActivityUpdates(string input)
        {
            string[] items = input.Split(".id=");

            List<string> objects = items
                .Where(item => !string.IsNullOrEmpty(item))
                .Select(item => $"id={item}")
                .ToList();

            return objects
                .Select(x => x.Split(';').ToList())
                .Select(arr =>
                {
                    var obj = new UserActivityUpdate();
                    var id = arr.Find(x => x.Contains("id")).Split('=')[1];
                    var handshake = arr.Find(x => x.Contains("handshake"))?.Split('=')[1] ?? "Never";
                    obj.Id = ParseEntityID(id);
                    obj.LastHandshake = handshake;
                    return obj;
                }).ToList();
        }
        #endregion

        public static async void HandleUserTraffics(List<DataUsage> updates, DBContext dbContext, IMikrotikRepository API, ILogger logger)
        {
            var dataUsages = await dbContext.DataUsages.ToListAsync();
            var existingItems = dataUsages.OrderBy(x => x.CreationTime).ToList();
            var lastKnownTraffics = dbContext.LastKnownTraffic.ToList();
            var users = await dbContext.Users.ToListAsync();

            foreach (var item in updates)
            {
                var tempUser = users.Find(x => x.Id == item.UserID);
                if (tempUser == null) continue;

                using var transactionDbContext = new DBContext(); // Create a new DbContext for each transaction
                using var transaction = await transactionDbContext.Database.BeginTransactionAsync();
                try
                {
                    LastKnownTraffic lastKnown = lastKnownTraffics.Find(x => x.UserID == item.UserID);
                    if (lastKnown == null) continue;

                    var old = existingItems.FindLast(oldItem => oldItem.UserID == item.UserID);
                    if (old == null)
                    {
                        await transactionDbContext.DataUsages.AddAsync(item);
                        tempUser.RX = item.RX + lastKnown.RX;
                        tempUser.TX = item.TX + lastKnown.TX;
                    }
                    else
                    {
                        if ((old.RX <= item.RX || old.TX <= item.TX) &&
                            (old.RX != item.RX && old.TX != item.TX)) // Normal Data (and not duplicate)
                        {
                            await transactionDbContext.DataUsages.AddAsync(item);
                        }
                        else if (old.RX > item.RX || old.TX > item.TX) // Server Reset
                        {
                            lastKnown.RX = old.RX;
                            lastKnown.TX = old.TX;
                            lastKnown.CreationTime = DateTime.Now;
                            transactionDbContext.LastKnownTraffic.Update(lastKnown);
                            item.ResetNotes = $"System reset detected at: {DateTime.Now}";
                            await transactionDbContext.DataUsages.AddAsync(item);
                        }
                        if (item.RX > old.RX) tempUser.RX = item.RX + lastKnown.RX;
                        if (item.TX > old.TX) tempUser.TX = item.TX + lastKnown.TX;
                    }
                    if (tempUser.TrafficLimit > 0 && tempUser.RX + tempUser.TX >= GigabyteToByte(tempUser.TrafficLimit))
                    {
                        // Disable User
                        logger.Information($"User #{tempUser.Id} reached {tempUser.RX + tempUser.TX} of {GigabyteToByte(tempUser.TrafficLimit)} bandwidth.");
                        var disable = await API.DisableUser(item.UserID);
                        if (disable.Code != "200")
                        {
                            logger.Error("Failed disabling user", new
                            {
                                userId = item.UserID,
                                disable.Code,
                                disable.Title,
                                disable.Description
                            });
                        }
                        else
                        {
                            logger.Information("Disabled user due to bandwidth limit", new
                            {
                                item.UserID,
                                TrafficUsed = ConvertByteSize(tempUser.RX + tempUser.TX),
                                tempUser.TrafficLimit
                            });
                        }
                    }
                    transactionDbContext.Users.Update(tempUser);
                    await transactionDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    logger.Error(ex.Message);
                    await transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    await transaction.RollbackAsync();
                }
            }
        }

        public static string GetProjectVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Return full path of requested file in app's home directory
        /// </summary>
        /// <param name="filename">requested file name</param>
        /// <returns></returns>
        public static string GetHomePath(string filename)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Join("/home/app", filename) : Path.Join(AppDomain.CurrentDomain.BaseDirectory, filename);
        }

        /// <summary>
        /// Return full path of requested file in log files directory
        /// </summary>
        /// <param name="filename">requested file name</param>
        /// <returns></returns>
        public static string GetLogPath(string filename) => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "log", filename);

        public static string GetIDFile() => GetHomePath("identifier.id");
        public static string GetIDContent()
        {
            var idFile = GetIDFile();

            if (!File.Exists(idFile))
            {
                using var fs = File.OpenWrite(idFile);
                var id = Guid.NewGuid().ToString();
                id = id[(id.LastIndexOf('-') + 1)..];
                byte[] identifier = new UTF8Encoding(true).GetBytes(id);
                fs.Write(identifier, 0, identifier.Length);
            }
            return File.ReadAllText(idFile);
        }

        public static Serilog.Core.Logger LoggerConfiguration()
        {
            return new LoggerConfiguration()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithRootName("Message").WithRootName("Exception").WithRootName("Exception"))
                .Enrich.WithProperty("App.Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0")
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithClientId(GetIDContent())
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(AspNetCoreRequestLogging())
                    .WriteTo.File(
                        GetLogPath("access.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31
                    ))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(LogEvent => LogEvent.Exception != null)
                    .WriteTo.Seq("https://mtwglogger.techgarage.ir/"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(AspNetCoreRequestLogging())
                    .WriteTo.SQLite(GetLogPath("logs.db")))
                .CreateLogger();
        }

        private static Func<LogEvent, bool> AspNetCoreRequestLogging()
        {
            return e =>
                    Matching.FromSource("Microsoft.AspNetCore.Hosting.Diagnostics").Invoke(e) ||
                    Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware").Invoke(e) ||
                    Matching.FromSource("Microsoft.AspNetCore.Routing.EndpointMiddleware").Invoke(e);
        }

        public static TimeSpan ConvertToTimeSpan(string input)
        {
            int weeks = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (input.Contains('w'))
            {
                string w = input.Split('w').First();
                weeks = int.Parse(w);
                input = input.Remove(0, input.IndexOf('w') + 1);
            }
            if (input.Contains('d'))
            {
                string d = input.Split('d').First();
                days = int.Parse(d);
                input = input.Remove(0, input.IndexOf('d') + 1);
            }
            if (input.Contains('h'))
            {
                string h = input.Split('h').First();
                hours = int.Parse(h);
                input = input.Remove(0, input.IndexOf('h') + 1);
            }
            if (input.Contains('m'))
            {
                string m = input.Split('m').First();
                minutes = int.Parse(m);
                input = input.Remove(0, input.IndexOf('m') + 1);
            }
            if (input.Contains('s'))
            {
                string s = input.Split('s').First();
                seconds = int.Parse(s);
            }

            return new TimeSpan((weeks * 7) + days, hours, minutes, seconds);
        }
    }

    public static class StringCompression
    {
        public static byte[] Compress(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using MemoryStream msi = new(bytes);
            using MemoryStream mso = new();
            using (GZipStream gs = new(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return mso.ToArray();
        }

        public static string Decompress(byte[] zip)
        {
            using MemoryStream msi = new(zip);
            using MemoryStream mso = new();
            using (GZipStream gs = new(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }

    public static partial class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static string RemoveNonNumerics(this string input) => Numerics().Replace(input, "");
        [GeneratedRegex("[^0-9.]")]
        private static partial Regex Numerics();
    }

    public static class ViewResultExtensions
    {
        public static string ToHtml(this ViewResult result, HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<IRoutingFeature>();
            var routeData = feature.RouteData;
            var viewName = result.ViewName ?? routeData.Values["action"] as string;
            var actionContext = new ActionContext(httpContext, routeData, new ControllerActionDescriptor());
            var options = httpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            var htmlHelperOptions = options.Value.HtmlHelperOptions;
            var viewEngineResult = result.ViewEngine?.FindView(actionContext, viewName, true) ?? options.Value.ViewEngines.Select(x => x.FindView(actionContext, viewName, true)).FirstOrDefault(x => x != null);
            var view = viewEngineResult.View;
            var builder = new StringBuilder();

            using (var output = new StringWriter(builder))
            {
                var viewContext = new ViewContext(actionContext, view, result.ViewData, result.TempData, output, htmlHelperOptions);

                view
                    .RenderAsync(viewContext)
                    .GetAwaiter()
                    .GetResult();
            }

            return builder.ToString();
        }
    }

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
