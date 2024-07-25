using Microsoft.Extensions.DependencyInjection;
using MTWireGuard.Application.Repositories;
using Serilog;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace MTWireGuard.Application
{
    public class SetupValidator(IServiceProvider serviceProvider)
    {
        private IMikrotikRepository api;
        private ILogger logger;

        public static bool IsValid { get; private set; }
        public static string Title { get; private set; }
        public static string Description { get; private set; }

        public async Task<bool> Validate()
        {
            InitializeServices();

            if (ValidateEnvironmentVariables())
            {
                LogAndDisplayError("Environment variables are not set!", "Please set \"MT_IP\", \"MT_USER\", \"MT_PASS\", \"MT_PUBLIC_IP\" variables in container environment.");
                IsValid = false;
                return false;
            }

            var (apiConnection, apiConnectionMessage) = await ValidateAPIConnection();
            if (!apiConnection)
            {
                var MT_IP = Environment.GetEnvironmentVariable("MT_IP");
                var ping = new Ping();
                var reply = ping.Send(MT_IP, 60 * 1000);
                if (reply.Status == IPStatus.Success)
                {
                    LogAndDisplayError("Error connecting to the router api!", apiConnectionMessage);
                }
                else
                {
                    LogAndDisplayError("Error connecting to the router api!", $"Can't find Mikrotik API server at address: {MT_IP}\r\nping status: {reply.Status}");
                }
                LogAndDisplayError("Error connecting to the router api!", apiConnectionMessage);
                IsValid = false;
                return false;
            }

            var ip = GetIPAddress();
            if (string.IsNullOrEmpty(ip))
            {
                LogAndDisplayError("Error getting container IP address!", "Invalid container IP address.");
                IsValid = false;
                return false;
            }

            if (!await api.TryConnectAsync())
            {
                LogAndDisplayError("Error connecting to the router api!", "Connecting to API failed.");
                IsValid = false;
                return false;
            }

            await EnsureTrafficScripts(ip);
            IsValid = true;
            return true;
        }

        private static bool ValidateEnvironmentVariables()
        {
            string? IP = Environment.GetEnvironmentVariable("MT_IP");
            string? USER = Environment.GetEnvironmentVariable("MT_USER");
            string? PASS = Environment.GetEnvironmentVariable("MT_PASS");
            string? PUBLICIP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP");

            return string.IsNullOrEmpty(IP) || string.IsNullOrEmpty(USER) || string.IsNullOrEmpty(PUBLICIP);
        }

        private async Task<(bool status, string? message)> ValidateAPIConnection()
        {
            try
            {
                return (await api.TryConnectAsync(), string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private string GetIPAddress()
        {
            try
            {
                var name = System.Net.Dns.GetHostName();
                var port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
                return System.Net.Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString() + $":{port}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting container IP address.");
                return string.Empty;
            }
        }

        private void LogAndDisplayError(string title, string description)
        {
            Title = title;
            Description = description;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[-] {Title}");
            Console.WriteLine($"[!] {Description}");
            Console.ResetColor();
            logger.Error("Error in container configuration", new { Error = Title, Description });
        }

        private void InitializeServices()
        {
            serviceProvider.GetService<DBContext>().Database.EnsureCreated();
            api = serviceProvider.GetService<IMikrotikRepository>();
            logger = serviceProvider.GetService<ILogger>();
        }

        private async Task EnsureTrafficScripts(string ip)
        {
            var scripts = await api.GetScripts();
            var schedulers = await api.GetSchedulers();

            //if (scripts.Find(x => x.Name == "SendTrafficUsage") == null)
            //{
            //    var create = await api.CreateScript(new()
            //    {
            //        Name = "SendTrafficUsage",
            //        Policies = ["write", "read", "test", "ftp"],
            //        DontRequiredPermissions = false,
            //        Source = Helper.PeersTrafficUsageScript($"http://{ip}/api/usage")
            //    });
            //    var result = create.Code;
            //    logger.Information("Created TrafficUsage Script", new
            //    {
            //        result = create
            //    });
            //}
            if (schedulers.Find(x => x.Name == "TrafficUsage") == null)
            {
                var create = await api.CreateScheduler(new()
                {
                    Name = "TrafficUsage",
                    Interval = new TimeSpan(0, 5, 0),
                    //OnEvent = "SendTrafficUsage",
                    OnEvent = Helper.PeersTrafficUsageScript($"http://{ip}/api/usage"),
                    Policies = ["write", "read", "test", "ftp"],
                    Comment = "update wireguard peers traffic usage"
                });
                var result = create.Code;
                logger.Information("Created TrafficUsage Scheduler", new
                {
                    result = create
                });
            }
        }

        private static void Shutdown()
        {
            Environment.Exit(0);
        }
    }
}
