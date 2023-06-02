using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "UpdateClientModal")]
    public class UpdateClientModalViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<WGServerViewModel> Servers)
        {
            ViewData["Servers"] = Servers;
            return View("UpdateClientModal", new UpdateClientRequest());
        }
    }
}
