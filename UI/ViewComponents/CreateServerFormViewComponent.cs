using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.ViewComponents
{
    [ViewComponent(Name = "CreateServerForm")]
    public class CreateServerFormViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("CreateServerForm", new CreateServerRequest());
        }
    }
}
