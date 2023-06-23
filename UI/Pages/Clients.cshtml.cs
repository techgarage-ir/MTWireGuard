using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Models.Requests;
using MTWireGuard.Models.Responses;

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
            var message = mapper.Map<ToastMessage>(make);
            return new ToastResult(message);
        }

        public async Task<IActionResult> OnPostDelete(DeleteRequest request)
        {
            var delete = await API.DeleteUser(request.Id);
            var message = mapper.Map<ToastMessage>(delete);
            return new ToastResult(message);
        }

        public async Task<IActionResult> OnPostUpdate(UpdateClientRequest request)
        {
            var model = mapper.Map<UserUpdateModel>(request);
            var update = await API.UpdateUser(model);
            var message = mapper.Map<ToastMessage>(update);
            return new ToastResult(message);
        }

        public async Task<IActionResult> OnPostSyncAsync(SyncUserRequest request)
        {
            var model = mapper.Map<UserSyncModel>(request);
            var update = await API.SyncUser(model);
            var message = mapper.Map<ToastMessage>(update);
            return new ToastResult(message);
        }

        public async Task<IActionResult> OnGetEnableAsync(ChangeStateRequest request)
        {
            CreationResult result;
            if (!request.Enabled)
                result = await API.EnableUser(request.Id);
            else
                result = await API.DisableUser(request.Id);
            var message = mapper.Map<ToastMessage>(result);
            return new ToastResult(message);
        }
    }
}
