using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Models;
using MTWireGuard.Models.Mikrotik;
using MTWireGuard.Models.Requests;
using MTWireGuard.Models.Responses;
using MTWireGuard.Repositories;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace MTWireGuard.Pages
{
    public class ClientsModel : PageModel
    {
        private readonly IMikrotikRepository API;
        private readonly IMapper mapper;

        public ClientsModel(IMikrotikRepository mikrotik, IMapper mapper)
        {
            API = mikrotik;
            this.mapper = mapper;
        }

        public async Task OnGetAsync()
        {
            var servers = await API.GetServersAsync();
            ViewData["servers"] = servers;
        }

        public async Task<PartialViewResult> OnGetGetAll()
        {
            var users = await API.GetUsersAsync();
            return Partial("_ClientsTable", users);
        }

        public async Task<IActionResult> OnGetQRAsync(int id)
        {
            string config = await API.GetQRCodeBase64(id);
            return Content(config);
        }

        public async Task<FileResult> OnGetDownloadTunnelAsync(int id)
        {
            string config = await API.GetUserTunnelConfig(id);

            byte[] bytesInStream = System.Text.Encoding.UTF8.GetBytes(config);

            var user = await API.GetUser(id);
            string filename = string.IsNullOrWhiteSpace(user.Name) ? user.Interface : user.Name;
            Response.Headers.Add("content-disposition", $"attachment; filename={filename}.conf");

            return File(bytesInStream, "text/plain");
        }

        public async Task<IActionResult> OnPostCreateAsync(CreateClientRequest request)
        {
            var model = mapper.Map<UserCreateModel>(request);
            var make = await API.CreateUser(model);
            string status = make.Code == "200" ? "success" : "danger";
            string title = make.Code == "200" ? make.Title : $"[{make.Code}] {make.Title}";
            return new ToastResult(title, make.Description, status);
        }

        public async Task<IActionResult> OnPostDelete(DeleteRequest request)
        {
            var delete = await API.DeleteUser(request.Id);
            string status = delete.Code == "200" ? "success" : "danger";
            string title = delete.Code == "200" ? delete.Title : $"[{delete.Code}] {delete.Title}";
            return new ToastResult(title, delete.Description, status);
        }

        public async Task<IActionResult> OnPostUpdate(UpdateClientRequest request)
        {
            var model = mapper.Map<UserUpdateModel>(request);
            var update = await API.UpdateUser(model);
            string status = update.Code == "200" ? "success" : "danger";
            string title = update.Code == "200" ? update.Title : $"[{update.Code}] {update.Title}";
            return new ToastResult(title, update.Description, status);
        }

        public async Task<IActionResult> OnPostSyncAsync(SyncUserRequest request)
        {
            var model = mapper.Map<UserSyncModel>(request);
            var update = await API.SyncUser(model);
            string status = update.Code == "200" ? "success" : "danger";
            string title = update.Code == "200" ? update.Title : $"[{update.Code}] {update.Title}";
            return new ToastResult(title, update.Description, status);
        }

        public async Task<IActionResult> OnGetEnableAsync(ChangeStateRequest request)
        {
            CreationResult result;
            if (!request.Enabled)
                result = await API.EnableUser(request.Id);
            else
                result = await API.DisableUser(request.Id);
            string status = result.Code == "200" ? "success" : "danger";
            string title = result.Code == "200" ? result.Title : $"[{result.Code}] {result.Title}";
            return new ToastResult(title, result.Description, status);
        }
    }
}
