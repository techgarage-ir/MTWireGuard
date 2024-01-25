using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTWireGuard.Application.MinimalAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

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

        public static string PeersLastHandshakeScript(string apiURL)
        {
            return $"/tool fetch mode=http url=\"{apiURL}\" http-method=post check-certificate=no http-data=([/interface/wireguard/peers/print show-ids proplist=last-handshake as-value]);";
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
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + ConvertByteSize(-value, decimalPlaces); }
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
                    obj.RX = int.Parse(rx);
                    obj.TX = int.Parse(tx);
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

        public static async void HandleUserTraffics(List<DataUsage> updates, DBContext dbContext, IMikrotikRepository API)
        {
            var dataUsages = await dbContext.DataUsages.ToListAsync();
            var existingItems = dataUsages.OrderBy(x => x.CreationTime).ToList();
            var lastKnownTraffics = dbContext.LastKnownTraffic.ToList();
            var users = await dbContext.Users.ToListAsync();
            foreach (var item in updates)
            {
                var tempUser = users.Find(x => x.Id == item.UserID);
                if (tempUser == null) continue;
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    LastKnownTraffic lastKnown = lastKnownTraffics.Find(x => x.UserID == item.UserID);
                    if (lastKnown == null) continue;

                    var old = existingItems.FindLast(oldItem => oldItem.UserID == item.UserID);
                    if (old == null)
                    {
                        await dbContext.DataUsages.AddAsync(item);
                        tempUser.RX = item.RX + lastKnown.RX;
                        tempUser.TX = item.TX + lastKnown.TX;
                    }
                    else
                    {
                        if ((old.RX <= item.RX || old.TX <= item.TX) &&
                            (old.RX != item.RX && old.TX != item.TX)) // Normal Data (and not duplicate)
                        {
                            await dbContext.DataUsages.AddAsync(item);
                        }
                        else if (old.RX > item.RX || old.TX > item.TX) // Server Reset
                        {
                            lastKnown.RX = old.RX;
                            lastKnown.TX = old.TX;
                            lastKnown.CreationTime = DateTime.Now;
                            dbContext.LastKnownTraffic.Update(lastKnown);
                            //if (lastKnown != null)
                            //{
                            //    dbContext.LastKnownTraffic.Update(lastKnown);
                            //}
                            //else
                            //{
                            //    await dbContext.LastKnownTraffic.AddAsync(lastKnown);
                            //}
                            item.ResetNotes = $"System reset detected at: {DateTime.Now}";
                            await dbContext.DataUsages.AddAsync(item);
                        }
                        if (item.RX > old.RX) tempUser.RX = item.RX + lastKnown.RX;
                        if (item.TX > old.TX) tempUser.TX = item.TX + lastKnown.TX;
                    }
                    if (tempUser.TrafficLimit > 0 && tempUser.RX + tempUser.TX >= tempUser.TrafficLimit)
                    {
                        // Disable User
                        var disable = await API.DisableUser(item.UserID);
                        if (disable.Code != "200")
                        {
                            Console.WriteLine("Failed disabling user");
                        }
                    }
                    dbContext.Users.Update(tempUser);
                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static string GetProjectVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
