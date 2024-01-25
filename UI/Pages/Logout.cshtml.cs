using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Pages
{
    public class LogoutModel(IMikrotikRepository api) : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string returnUrl = "Login")
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            var sessionId = await api.GetCurrentSessionID();
            var kill = await api.KillJob(sessionId);
            return RedirectToPage(returnUrl);
        }
    }
}
