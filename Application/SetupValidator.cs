using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MTWireGuard.Application
{
    public class SetupValidator(IServiceProvider serviceProvider)
    {
        private IMikrotikRepository api;

        public async Task Validate()
        {
            var envVariables = ValidateEnvironmentVariables();
            if (envVariables)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] Environment variables are not set!");
                Console.WriteLine($"[!] Please set \"MT_IP\", \"MT_USER\", \"MT_PASS\", \"MT_PUBLIC_IP\" variables in container environment.");
                Console.ResetColor();
                Shutdown();
            }

            serviceProvider.GetService<DBContext>().Database.EnsureCreated();
            api = serviceProvider.GetService<IMikrotikRepository>();

            var (apiConnection, apiConnectionMessage) = await ValidateAPIConnection();
            if (!apiConnection)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] Error connecting to the router api!");
                Console.WriteLine($"[!] {apiConnectionMessage}");
                Console.ResetColor();
                Shutdown();
            }

            var ip = GetIPAddress();
            if (string.IsNullOrEmpty(ip))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] Error getting container IP address!");
                Console.ResetColor();
                Shutdown();
            }
            var scripts = await api.GetScripts();
            var schedulers = await api.GetSchedulers();
            var trafficScript = scripts.Find(x => x.Name == "SendTrafficUsage");
            var trafficScheduler = schedulers.Find(x => x.Name == "TrafficUsage");

            if (trafficScript == null)
            {
                var create = await api.CreateScript(new()
                {
                    Name = "SendTrafficUsage",
                    Policies = ["write", "read", "test"],
                    DontRequiredPermissions = false,
                    Source = Helper.PeersTrafficUsageScript($"http://{ip}/api/usage")
                });
                var result = create.Code;
            }
            if (trafficScheduler == null)
            {
                var create = await api.CreateScheduler(new()
                {
                    Name = "TrafficUsage",
                    Interval = new TimeSpan(0, 5, 0),
                    OnEvent = "SendTrafficUsage",
                    Policies = ["write", "read", "test"]
                });
                var result = create.Code;
            }
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

        private static string GetIPAddress()
        {
            try
            {
                var name = System.Net.Dns.GetHostName();
                var port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
                return System.Net.Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString() + $":{port}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private static void Shutdown()
        {
            Environment.Exit(0);
        }
    }
}
