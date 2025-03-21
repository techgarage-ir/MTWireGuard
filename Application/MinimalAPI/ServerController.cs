﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Repositories;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Serilog;

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

        public static async Task Import(
            HttpContext context,
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromServices] ILogger logger)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                try
                {
                    // Buffer to receive the data
                    var buffer = new byte[1024 * 1024]; // Adjust buffer size 1MB
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Convert the received data into ImportServersRequest
                        var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var request = JsonSerializer.Deserialize<ImportServersRequest>(jsonMessage, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (request == null)
                        {
                            logger.Warning("Invalid data received, {data}", jsonMessage);
                            await webSocket.CloseAsync(result?.CloseStatus.Value ?? WebSocketCloseStatus.InvalidPayloadData, result.CloseStatusDescription, CancellationToken.None);
                        }

                        // Map the request to the model
                        var model = mapper.Map<List<ServerImportModel>>(request.Servers);
                        var make = API.ImportServers(model, webSocket).Result;
                        var message = mapper.Map<ToastMessage>(make);
                        await webSocket.CloseAsync(make.Code == "200" ? WebSocketCloseStatus.NormalClosure : WebSocketCloseStatus.InternalServerError, result.CloseStatusDescription, CancellationToken.None);
                    }
                    else
                    {
                        logger.Warning("Invalid message type, {wsResult}", result);
                        await webSocket.CloseAsync(result?.CloseStatus.Value ?? WebSocketCloseStatus.InvalidMessageType, result.CloseStatusDescription, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error handling WebSocket");
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
                }
            }
            else
            {
                logger.Warning("Not a WebSocket request {request}", context.Request);
            }
        }
    }
}
