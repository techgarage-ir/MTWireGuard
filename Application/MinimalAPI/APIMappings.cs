using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MikrotikAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace MTWireGuard.Application.MinimalAPI
{
    public static class APIMappings
    {
        public static RouteGroupBuilder MapGeneralApi(this RouteGroupBuilder group)
        {
            // Retreive updates from Mikrotik
            group.MapPost(Endpoints.Usage, TrafficUsageUpdate);
            group.MapGet(Endpoints.IPLookup, IPLookup)
                .RequireAuthorization();
            group.MapGet(Endpoints.CheckUpdate, CheckForUpdates)
                .RequireAuthorization();
            // Map auth endpoints
            group.MapGroup(Endpoints.Auth)
                .MapAuthAPI();
            // Map wireguard users endpoints
            group.MapGroup(Endpoints.User)
                .MapUserApi()
                .RequireAuthorization();
            // Map wireguard servers endpoints
            group.MapGroup(Endpoints.Server)
                .MapServerApi()
                .RequireAuthorization();
            // Map IP pool endpoints
            group.MapGroup(Endpoints.IPPool)
                .MapIPPoolsApi()
                .RequireAuthorization();
            // Map configuration endpoints
            group.MapGroup(Endpoints.Configuration)
                .MapConfigurationApi()
                .RequireAuthorization();

            return group;
        }
        private static RouteGroupBuilder MapAuthAPI(this RouteGroupBuilder group)
        {
            group.MapGet(Endpoints.Logout, AuthController.Logout);
            group.MapPost(Endpoints.Login, AuthController.Login);
            return group;
        }
        private static RouteGroupBuilder MapUserApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", UserController.GetAll);
            group.MapGet("/{id}", UserController.GetById);
            group.MapGet($"{Endpoints.Count}", UserController.GetCount);
            group.MapGet($"{Endpoints.QR}/{{id}}", UserController.GetQR);
            group.MapGet($"{Endpoints.File}/{{id}}", UserController.GetFile);
            group.MapGet($"{Endpoints.Onlines}", UserController.GetOnlines);
            group.MapGet($"{Endpoints.Reset}/{{id}}", UserController.ResetTraffic);
            group.MapGet($"{Endpoints.V2ray}/{{id}}", UserController.GetV2rayQR);
            group.MapPost("/", UserController.Create);
            group.MapPut("/{id}", UserController.Update);
            group.MapPatch($"{Endpoints.Activation}/{{id}}", UserController.Activation);
            group.MapDelete("/{id}", UserController.Delete);
            group.Map($"{Endpoints.Import}", UserController.Import);

            return group;
        }
        private static RouteGroupBuilder MapServerApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", ServerController.GetAll);
            group.MapGet($"{Endpoints.Count}", ServerController.GetCount);
            group.MapPost("/", ServerController.Create);
            group.MapPut("/{id}", ServerController.Update);
            group.MapDelete("/{id}", ServerController.Delete);
            group.MapPatch($"{Endpoints.Activation}/{{id}}", ServerController.Activation);
            group.Map($"{Endpoints.Import}", ServerController.Import);

            return group;
        }
        private static RouteGroupBuilder MapIPPoolsApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", IPPoolController.GetAll);
            group.MapPost("/", IPPoolController.Create);
            group.MapPut("/{id}", IPPoolController.Update);
            group.MapDelete("/{id}", IPPoolController.Delete);

            return group;
        }
        private static RouteGroupBuilder MapConfigurationApi(this RouteGroupBuilder group)
        {
            group.MapGet(Endpoints.Logs, async ([FromServices] IMikrotikRepository API) => await API.GetLogsAsync()).
                RequireAuthorization();
            group.MapGet(Endpoints.Resources, ConfigurationController.Resources).
                RequireAuthorization();
            group.MapGet(Endpoints.DNS, ConfigurationController.DNS)
                .RequireAuthorization();
            group.MapPut(Endpoints.DNS, ConfigurationController.DNSUpdate)
                .RequireAuthorization();
            group.MapGet(Endpoints.Identity, ConfigurationController.Identity)
                .RequireAuthorization();
            group.MapPut(Endpoints.Identity, ConfigurationController.IdentityUpdate)
                .RequireAuthorization();
            group.MapGet(Endpoints.Information, ConfigurationController.Information)
                .RequireAuthorization();
            return group;
        }

        /// <summary>
        /// Retrieve and handle WG peer traffic usage
        /// </summary>
        public static async Task<Results<Accepted, ProblemHttpResult>> TrafficUsageUpdate(
            [FromServices] IMapper mapper,
            [FromServices] Serilog.ILogger logger,
            [FromServices] DBContext dbContext,
            [FromServices] IMikrotikRepository mikrotikRepository,
            HttpContext context)
        {
            StreamReader reader = new(context.Request.Body);
            string body = await reader.ReadToEndAsync();

            var list = TrafficUtil.ParseTrafficUsage(body);
            var updates = mapper.Map<List<DataUsage>>(list);

            if (updates == null || updates.Count < 1) return TypedResults.Problem("Empty data");

            TrafficUtil.HandleUserTraffics(updates, dbContext, mikrotikRepository, logger);

            return TypedResults.Accepted("Done");
        }

        /// <summary>
        /// Retrieve IP Geo info
        /// </summary>
        public static async Task<Ok<IPLookup>> IPLookup(
            [FromServices] Serilog.ILogger logger,
            [FromServices] IMapper mapper,
            HttpContext context)
        {
            string? ip = Environment.GetEnvironmentVariable("MT_PUBLIC_IP");
            if (string.IsNullOrEmpty(ip))
            {
                logger.Error("MT_PUBLIC_IP is not set.");
                return TypedResults.Ok(new IPLookup());
            }
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://ip-api.com/json/{ip}?fields=50689");
            var result = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(result);
            JSchema schema = Constants.IPApiSchema;
            var info = json.IsValid(schema) ? mapper.Map<IPLookup>(result.ToModel<IPAPIResponse>()) : mapper.Map<IPLookup>(result.ToModel<IPAPIFailResponse>());
            return TypedResults.Ok(info);
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        public static async Task<string> CheckForUpdates(
            [FromServices] Serilog.ILogger logger)
        {
            try
            {
                var url = "https://api.github.com/repos/techgarage-ir/MTWireguard/tags";
                var currentVersionString = CoreUtil.GetProjectVersion();
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"MTWireguard ${currentVersionString}");
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                JArray tags = JArray.Parse(json);
                var latestTag = tags.FirstOrDefault();
                if (latestTag != null)
                {
                    var latestVersionString = latestTag["name"].Value<string>();
                    Version latestVersion = new(latestVersionString[1..]);
                    Version currentVersion = new(currentVersionString);

                    switch (currentVersion)
                    {
                        case Version expression when currentVersion < latestVersion:
                            return $"New version ({latestVersionString}) available.";
                        case Version expression when currentVersion == latestVersion:
                            return "You are using latest version.";
                        default:
                            logger.Error($"Invalid version detected! Using version: {currentVersionString} while the latest known version is: {latestVersionString}.");
                            return "Invalid version number!";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking for updates!");
            }
            return "ERROR checking for new version!";
        }
    }
    internal static class Endpoints
    {
        // Groups
        public const string Auth = "/Auth";
        public const string User = "/Users";
        public const string Server = "/Servers";
        public const string IPPool = "/IPPools";
        public const string Configuration = "/Config";

        // Endpoints
        //  Auth
        public const string Login = "/Login";
        public const string Logout = "/Logout";
        //  Users, Servers
        public const string Activation = "/Activation";
        public const string Count = "/Count";
        public const string Import = "/Import";
        public const string File = "/File";
        public const string QR = "/QR";
        public const string Onlines = "/Onlines";
        public const string Reset = "/Reset";
        public const string V2ray = "/V2ray";
        //  Configuration
        public const string DNS = "/DNS";
        public const string Identity = "/Identity";
        public const string Logs = "/Logs";
        public const string Resources = "/Resources";
        public const string Information = "/Information";
        //  Retrival
        public const string Usage = "/Usage";
        public const string IPLookup = "/IPLookup";
        public const string CheckUpdate = "/CheckUpdates";
    }
}
