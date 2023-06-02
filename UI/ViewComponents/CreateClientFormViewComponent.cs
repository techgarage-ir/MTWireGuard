using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;
using System.Xml.Linq;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "CreateClientForm")]
    public class CreateClientFormViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("CreateClientForm", new CreateClientRequest());
        }
    }
}
