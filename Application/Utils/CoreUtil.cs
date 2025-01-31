using MTWireGuard.Application.Models.Mikrotik;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Filters;
using System.Runtime.InteropServices;
using System.Text;

namespace MTWireGuard.Application.Utils
{
    public static class CoreUtil
    {
        /// <summary>
        /// Return application version as string
        /// </summary>
        /// <returns></returns>
        public static string GetProjectVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
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

        /// <summary>
        /// Serilog configuration
        /// </summary>
        /// <returns></returns>
        public static Serilog.Core.Logger LoggerConfiguration()
        {
            return new LoggerConfiguration()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithRootName("Message").WithRootName("Exception").WithRootName("Exception"))
                .Enrich.WithCallerInfo(
                    includeFileInfo: true,
                    assemblyPrefix: "MTWireGuard.",
                    prefix: "Log.Source_",
                    filePathDepth: 10)
                .Enrich.WithProperty("App.Version", GetProjectVersion())
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithClientId(GetIDContent())
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(AspNetCoreRequestLogging())
                    .WriteTo.SQLite(GetLogPath("logs.db")))
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
                .CreateLogger();
        }

        /// <summary>
        /// HTTP requests logging sources
        /// </summary>
        /// <returns></returns>
        private static Func<LogEvent, bool> AspNetCoreRequestLogging()
        {
            return e =>
                    Matching.FromSource("Microsoft.AspNetCore.Hosting.Diagnostics").Invoke(e) ||
                    Matching.FromSource("Microsoft.AspNetCore.Routing.EndpointMiddleware").Invoke(e) ||
                    Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware").Invoke(e);
        }

        /// <summary>
        /// Filter peers with handshake less than 2 minutes
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static List<WGPeerLastHandshakeViewModel> FilterOnlineUsers(List<WGPeerLastHandshakeViewModel> users) => users.Where(u => u.LastHandshake < new TimeSpan(0, 2, 1)).ToList();
    }
}
