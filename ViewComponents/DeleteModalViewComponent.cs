using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "DeleteModal")]
    public class DeleteModalViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(bool IsServer)
        {
            ViewData["IsServer"] = IsServer;
            return View("DeleteModal", new DeleteRequest());
        }
    }
}
