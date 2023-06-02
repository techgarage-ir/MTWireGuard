using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IMikrotikRepository API;

        public LogoutModel(IMikrotikRepository mikrotik)
        {
            API = mikrotik;
        }
        public async Task<IActionResult> OnGetAsync(string returnUrl = "Login")
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            /*
            var sessionId = await MTAPIHandler.GetCurrentSessionID();
            var remove = await MTAPIHandler.KillJob(sessionId);*/
            var sessionId = await API.GetCurrentSessionID();
            var kill = await API.KillJob(sessionId);
            return RedirectToPage(returnUrl);
        }
    }
}
