using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "UpdateClientModal")]
    public class UpdateClientModalViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<Models.Mikrotik.WGServerViewModel> Servers)
        {
            ViewData["Servers"] = Servers;
            return View("UpdateClientModal", new UpdateClientRequest());
        }
    }
}
