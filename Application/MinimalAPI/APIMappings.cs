using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MikrotikAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTWireGuard.Application.MinimalAPI
{
    public static class APIMappings
    {
        public static RouteGroupBuilder MapGeneralApi(this RouteGroupBuilder group)
        {
            // Retreive updates from Mikrotik
            group.MapPost(Endpoints.Usage, TrafficUsageUpdate);
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
            group.MapGet($"{Endpoints.QR}/{{id}}", UserController.GetQR);
            group.MapGet($"{Endpoints.File}/{{id}}", UserController.GetFile);
            group.MapPost("/", UserController.Create);
            group.MapPut("/{id}", UserController.Update);
            group.MapPatch($"{Endpoints.Sync}/{{id}}", UserController.Sync);
            group.MapPatch($"{Endpoints.Activation}/{{id}}", UserController.Activation);
            group.MapDelete("/{id}", UserController.Delete);

            return group;
        }
        private static RouteGroupBuilder MapServerApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", ServerController.GetAll);
            group.MapPost("/", ServerController.Create);
            group.MapPut("/{id}", ServerController.Update);
            group.MapDelete("/{id}", ServerController.Delete);
            group.MapPatch($"{Endpoints.Activation}/{{id}}", ServerController.Activation);

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
            [FromServices] DBContext dbContext,
            [FromServices] IMikrotikRepository mikrotikRepository,
            HttpContext context)
        {

            StreamReader reader = new(context.Request.Body);
            string body = await reader.ReadToEndAsync();

            var list = Helper.ParseTrafficUsage(body);
            var updates = mapper.Map<List<DataUsage>>(list);

            if (updates == null || updates.Count < 1) return TypedResults.Problem("Empty data");

            Helper.HandleUserTraffics(updates, dbContext, mikrotikRepository);

            return TypedResults.Accepted("Done");
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
        public const string File = "/File";
        public const string QR = "/QR";
        public const string Sync = "/Sync";
        //  Configuration
        public const string DNS = "/DNS";
        public const string Identity = "/Identity";
        public const string Logs = "/Logs";
        public const string Resources = "/Resources";
        public const string Information = "/Information";
        //  Retrival
        public const string Usage = "/Usage";
    }
}
