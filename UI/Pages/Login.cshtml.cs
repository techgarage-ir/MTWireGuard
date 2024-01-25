using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application;

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
