using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Pages
{
    public class LogsModel : PageModel
    {
        private readonly IMikrotikRepository API;
        
        public LogsModel(IMikrotikRepository mikrotik)
        {
            API = mikrotik;
        }

        public async Task OnGetAsync()
        {
            ViewData["Logs"] = await API.GetLogsAsync();
        }
    }
}
