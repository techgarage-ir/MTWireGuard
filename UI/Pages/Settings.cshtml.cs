using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;

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
            var ramUsed = 100 - info.FreeRAMPercentage;
            var hddUsed = 100 - info.FreeHDDPercentage;
            string cpuColor, ramColor, hddColor;

            if (info.CPULoad <= 25) cpuColor = "bg-info-gradient";
            else if (info.CPULoad <= 75) cpuColor = "bg-warning-gradient";
            else cpuColor = "bg-danger-gradient";

            if (hddUsed <= 25) hddColor = "bg-info-gradient";
            else if (hddUsed <= 75) hddColor = "bg-warning-gradient";
            else hddColor = "bg-danger-gradient";

            if (ramUsed <= 25) ramColor = "bg-info-gradient";
            else if (ramUsed <= 75) ramColor = "bg-warning-gradient";
            else ramColor = "bg-danger-gradient";

            var result = new SidebarInfo()
            {
                CPUBgColor = cpuColor,
                HDDBgColor = hddColor,
                RAMBgColor = ramColor,
                CPUUsedPercentage = info.CPULoad,
                HDDUsedPercentage = hddUsed,
                RAMUsedPercentage = ramUsed,
                HDDUsed = info.UsedHDD,
                RAMUsed = info.UsedRAM,
                TotalHDD = info.TotalHDD,
                TotalRAM = info.TotalRAM,
            };
            return new JsonResult(result);
        }
    }
}
