using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application;

namespace MTWireGuard.Pages
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ClientModel(IMikrotikRepository api) : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            var user = HttpContext.Session.Get<WGPeerViewModel>("user");
            if (user == null)
            {
                return RedirectPermanent("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync([FromBody] LoginRequest login)
        {
            var users = await api.GetUsersAsync();
            if (users != null)
            {
                var user = users.Find(x => x.Name.Equals(login.Username, StringComparison.CurrentCultureIgnoreCase));
                if (user != null)
                {
                    HttpContext.Session.Set("user", user);
                    return Page();
                }
            }
            return Unauthorized();
        }
    }
}
