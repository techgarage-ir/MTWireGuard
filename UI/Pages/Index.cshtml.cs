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
        public void OnGet()
        {
        }
    }
}