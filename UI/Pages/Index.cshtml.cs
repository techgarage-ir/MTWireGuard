using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application;
using MTWireGuard.Application.Repositories;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MTWireGuard.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IMikrotikRepository API;

        public IndexModel(ILogger<IndexModel> logger, IMikrotikRepository mikrotik)
        {
            _logger = logger;
            API = mikrotik;
        }

        public async Task OnGetAsync()
        {
            var users = await API.GetUsersAsync();
            var servers = await API.GetServersAsync();
            var traffics = await API.GetServersTraffic();
            var wgTraffics = traffics.Where(s => s.Type == "wg");
            long up = 0;
            long down = 0;
            foreach (var item in wgTraffics)
            {
                up += item.UploadBytes;
                down += item.DownloadBytes;
            }
            ViewData["users"] = users;
            ViewData["servers"] = servers;
            ViewData["uptr"] = Helper.ConvertByteSize(up, 2);
            ViewData["downtr"] = Helper.ConvertByteSize(down, 2);
        }
    }
}