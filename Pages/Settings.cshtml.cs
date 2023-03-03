using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Repositories;

namespace MTWireGuard.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly IMikrotikRepository API;

        public SettingsModel(IMikrotikRepository mikrotik)
        {
            API = mikrotik;
        }

        public async Task OnGetAsync()
        {
            ViewData["servers"] = await API.GetServersAsync();
            ViewData["info"] = await API.GetInfo();
            var identity = await API.GetName();
            ViewData["name"] = identity.Name;
        }

        public async Task<IActionResult> OnGetGetInfo()
        {
            var info = await API.GetInfo();
            return new JsonResult(info);
        }
    }
}
