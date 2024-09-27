using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Application.MinimalAPI
{
    internal class ServerController
    {
        public static async Task<Results<Ok<List<WGServerViewModel>>, NotFound>> GetAll([FromServices] IMikrotikRepository API)
        {
            var servers = await API.GetServersAsync();
            return servers.Any() ? TypedResults.Ok(servers) : TypedResults.NotFound();
        }

        public static async Task<Ok<WGServerStatistics>> GetCount([FromServices] IMikrotikRepository API)
        {
            var servers = await API.GetServersCount();
            return TypedResults.Ok(servers);
        }

        public static async Task<Ok<ToastMessage>> Create(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromBody] CreateServerRequest request)
        {
            var model = mapper.Map<ServerCreateModel>(request);
            var make = await API.CreateServer(model);
            var message = mapper.Map<ToastMessage>(make);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Update(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            int id,
            [FromBody] UpdateServerRequest request)
        {
            request.Id = id;
            var model = mapper.Map<ServerUpdateModel>(request);
            var update = await API.UpdateServer(model);
            var message = mapper.Map<ToastMessage>(update);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Delete(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromRoute] int id)
        {
            var delete = await API.DeleteServer(id);
            var message = mapper.Map<ToastMessage>(delete);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Activation(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromRoute] int id,
            ChangeStateRequest request)
        {
            request.Id = id;
            var active = (!request.Enabled) ? await API.EnableServer(request.Id) : await API.DisableServer(request.Id);
            var message = mapper.Map<ToastMessage>(active);
            return TypedResults.Ok(message);
        }
    }
}
