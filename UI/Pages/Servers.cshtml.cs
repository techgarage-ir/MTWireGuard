using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Models.Requests;
using MTWireGuard.Models.Responses;
using MTWireGuard.Application.Repositories;
using Newtonsoft.Json;

namespace MTWireGuard.Pages
{
    public class ServersModel : PageModel
    {
        private readonly IMikrotikRepository API;
        private readonly IMapper mapper;

        public ServersModel(IMikrotikRepository mikrotik, IMapper mapper)
        {
            API = mikrotik;
            this.mapper = mapper;
        }

        public async Task OnGetAsync()
        {
        }

        public async Task<PartialViewResult> OnGetGetAll()
        {
            var servers = await API.GetServersAsync();
            var traffics = await API.GetServersTraffic();
            return Partial("_ServersTable", (servers, traffics));
        }

        public async Task<IActionResult> OnPostCreateAsync(CreateServerRequest request)
        {
            var model = mapper.Map<ServerCreateModel>(request);
            var make = await API.CreateServer(model);
            var message = mapper.Map<ToastMessage>(make);
            return new ToastResult(message);
            /*
            string status = make.Code == "200" ? "success" : "danger";
            string title = make.Code == "200" ? make.Title : $"[{make.Code}] {make.Title}";
            return new ToastResult(title, make.Description, status);*/
        }

        public async Task<IActionResult> OnPostDelete(DeleteRequest request)
        {
            var delete = await API.DeleteServer(request.Id);
            var message = mapper.Map<ToastMessage>(delete);
            return new ToastResult(message);
            /*
            string status = delete.Code == "200" ? "success" : "danger";
            string title = delete.Code == "200" ? delete.Title : $"[{delete.Code}] {delete.Title}";
            return new ToastResult(title, delete.Description, status);*/
        }

        public async Task<IActionResult> OnPostUpdate(UpdateServerRequest request)
        {
            var model = mapper.Map<ServerUpdateModel>(request);
            var update = await API.UpdateServer(model);
            var message = mapper.Map<ToastMessage>(update);
            return new ToastResult(message);
            /*
            string status = update.Code == "200" ? "success" : "danger";
            string title = update.Code == "200" ? update.Title : $"[{update.Code}] {update.Title}";
            return new ToastResult(title, update.Description, status);*/
        }

        public async Task<IActionResult> OnGetEnableAsync(ChangeStateRequest request)
        {
            CreationResult result;
            if (!request.Enabled)
                result = await API.EnableServer(request.Id);
            else
                result = await API.DisableServer(request.Id);
            var message = mapper.Map<ToastMessage>(result);
            return new ToastResult(message);
            /*
            string status = result.Code == "200" ? "success" : "danger";
            string title = result.Code == "200" ? result.Title : $"[{result.Code}] {result.Title}";
            return new ToastResult(title, result.Description, status);*/
        }
    }
}
