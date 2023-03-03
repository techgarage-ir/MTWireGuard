using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "UpdateServerModal")]
    public class UpdateServerModalViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("UpdateServerModal", new UpdateServerRequest());
        }
    }
}