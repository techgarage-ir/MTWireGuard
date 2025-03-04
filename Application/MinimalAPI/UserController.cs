using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Utils;
using Serilog;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MTWireGuard.Application.MinimalAPI
{
    internal class UserController
    {
        public static async Task<Results<Ok<List<WGPeerViewModel>>, NotFound>> GetAll([FromServices] IMikrotikRepository API, HttpContext context)
        {
            var users = await API.GetUsersAsync();
            if (users.Count > 0)
                return TypedResults.Ok(users);
            return TypedResults.NotFound();
        }

        public static async Task<Results<Ok<WGPeerViewModel>, NotFound>> GetById([FromServices] IMikrotikRepository API, int id)
        {
            var user = await API.GetUser(id);
            if (user != null)
                return TypedResults.Ok(user);
            return TypedResults.NotFound();
        }

        public static async Task<Ok<List<WGPeerLastHandshakeViewModel>>> GetOnlines([FromServices] IMikrotikRepository API)
        {
            var users = await API.GetUsersHandshakes();
            var onlines = CoreUtil.FilterOnlineUsers(users);
            return TypedResults.Ok(onlines);
        }

        public static async Task<Ok<WGUserStatistics>> GetCount([FromServices] IMikrotikRepository API)
        {
            var users = await API.GetUsersCount();
            return TypedResults.Ok(users);
        }

        public static async Task<Ok<string>> GetQR([FromServices] IMikrotikRepository API, int id)
        {
            string config = await API.GetQRCodeBase64(id);
            return TypedResults.Ok(config);
        }

        public static async Task<FileContentHttpResult> GetFile([FromServices] IMikrotikRepository API, int id)
        {
            string config = await API.GetUserTunnelConfig(id);

            byte[] bytesInStream = Encoding.UTF8.GetBytes(config);

            var user = await API.GetUser(id);
            string filename = string.IsNullOrWhiteSpace(user.Name) ? user.Interface : user.Name;
            return TypedResults.File(
                fileContents: bytesInStream,
                fileDownloadName: filename + ".conf");
        }

        public static async Task<Ok<ToastMessage>> Create(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromBody] CreateClientRequest request)
        {
            var model = mapper.Map<UserCreateModel>(request);
            var make = await API.CreateUser(model);
            var message = mapper.Map<ToastMessage>(make);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Update(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            int id,
            UpdateClientRequest request)
        {
            request.ID = id;
            var model = mapper.Map<UserUpdateModel>(request);
            var update = await API.UpdateUser(model);
            var message = mapper.Map<ToastMessage>(update);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> Delete(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            [FromRoute] int id)
        {
            var delete = await API.DeleteUser(id);
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
            var active = (!request.Enabled) ? await API.EnableUser(request.Id) : await API.DisableUser(request.Id);
            var message = mapper.Map<ToastMessage>(active);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<ToastMessage>> ResetTraffic(
            [FromServices] IMikrotikRepository API,
            [FromServices] IMapper mapper,
            int id)
        {
            var reset = await API.ResetUserTraffic(id);
            var message = mapper.Map<ToastMessage>(reset);
            return TypedResults.Ok(message);
        }

        public static async Task<Ok<string>> GetV2rayQR(
            [FromServices] IMikrotikRepository API,
            int id)
        {
            string config = await API.GetUserV2rayQRCodeBase64(id);
            return TypedResults.Ok(config);
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
                        // Convert the received data into ImportUsersRequest
                        var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var request = JsonSerializer.Deserialize<ImportUsersRequest>(jsonMessage, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (request == null)
                        {
                            logger.Warning("Invalid data received, {data}", jsonMessage);
                            await webSocket.CloseAsync(result?.CloseStatus.Value ?? WebSocketCloseStatus.InvalidPayloadData, result.CloseStatusDescription, CancellationToken.None);
                        }

                        // Map the request to the model
                        var model = mapper.Map<List<UserImportModel>>(request.Users);
                        var make = API.ImportUsers(model, webSocket).Result;
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
                    logger.Error("Error handling WebSocket", ex);
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
