using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Utils;

namespace MTWireGuard.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string ReturnUrl = "Index")
        {
            var user = HttpContext.Session.Get<WGPeerViewModel>("user");
            if (user != null)
            {
                return RedirectPermanent("/Client");
            }
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                ReturnUrl = ReturnUrl == "/" ? "Index" : ReturnUrl;
                return RedirectToPage(ReturnUrl);
            }
            return Page();
        }
    }
}
