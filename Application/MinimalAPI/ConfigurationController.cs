using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Application.MinimalAPI
{
    internal class ConfigurationController
    {
        public static async Task<Ok<object>> Resources(
            [FromServices] IMikrotikRepository API)
        {
            var info = await API.GetInfo();
            var ramUsed = 100 - info.FreeRAMPercentage;
            var hddUsed = 100 - info.FreeHDDPercentage;

            var output = new JsonResult(new
            {
                HDD = new
                {
                    Total = info.TotalHDD,
                    Used = info.UsedHDD,
                    Free = info.FreeHDD,
                    Percentage = Convert.ToByte(hddUsed)
                },
                RAM = new
                {
                    Total = info.TotalRAM,
                    Used = info.UsedRAM,
                    Free = info.FreeRAM,
                    Percentage = Convert.ToByte(ramUsed)
                },
                info.CPULoad,
                info.UPTime
            }).Value;
            return TypedResults.Ok(output);
        }

        public static async Task<Results<Ok<DNS>, ProblemHttpResult>> DNS(
            [FromServices] IMikrotikRepository API)
        {
            return TypedResults.Ok(await API.GetDNS());
        }

        public static async Task<Ok<object>> Information(
            [FromServices] IMikrotikRepository API)
        {
            var info = await API.GetInfo();
            var identity = await API.GetName();
            var dns = await API.GetDNS();

            var dnsValues = new List<string>();
            dnsValues.AddRange(dns.Servers.Split(','));
            dnsValues.AddRange(dns.DynamicServers.Split(','));

            var output = new JsonResult(new
            {
                Identity = identity.Name,
                DNS = dnsValues,
                Device = new
                {
                    info.BoardName,
                    info.Architecture
                },
                info.Version,
                IP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP")
            }).Value;
            return TypedResults.Ok(output);
        }

        public static async Task<Results<Ok<IdentityViewModel>, ProblemHttpResult>> Identity(
            [FromServices] IMikrotikRepository API)
        {
            return TypedResults.Ok(await API.GetName());
        }

        public static async Task<Ok<ToastMessage>> IdentityUpdate(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromBody] UpdateIdentityRequest request)
        {
            var model = mapper.Map<IdentityUpdateModel>(request);
            var update = await API.SetName(model);
            var message = mapper.Map<ToastMessage>(update);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> DNSUpdate(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromBody] UpdateDNSRequest request)
        {
            var model = mapper.Map<DNSUpdateModel>(request);
            model.Servers.Remove(string.Empty);
            model.Servers.Remove(" ");
            var update = await API.SetDNS(model);
            var message = mapper.Map<ToastMessage>(update);
            return TypedResults.Ok(message);
        }
    }
}
