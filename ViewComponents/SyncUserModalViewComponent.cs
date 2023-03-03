using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;
using System.Xml.Linq;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "SyncUserModal")]
    public class SyncUserModalViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("SyncUserModal", new SyncUserRequest());
        }
    }
}
