using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MTWireGuard.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToPage("Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string username, string password)
        {
            try
            {
                string MT_IP = Environment.GetEnvironmentVariable("MT_IP");
                string MT_USER = Environment.GetEnvironmentVariable("MT_USER");
                string MT_PASS = Environment.GetEnvironmentVariable("MT_PASS");

                if (username == MT_USER && password == MT_PASS)
                {
                    HttpClientHandler handler = new()
                    {
                        ServerCertificateCustomValidationCallback = delegate { return true; }
                    };
                    using HttpClient httpClient = new(handler);
                    using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://{MT_IP}/rest/");
                    string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    var resp = await response.Content.ReadAsStringAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, "Administrator"),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true
                    };

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal,
                        authProperties);
                    HttpContext.User = claimsPrincipal;

                    return RedirectToPage("Index");
                }
                else
                {
                    ViewData["Username"] = username;
                    ViewData["Invalid"] = true;
                }
            }
            catch (Exception ex)
            {
                ViewData["body"] = ex.Message;
                Console.WriteLine(ex.Message);
            }
            return Page();
        }
    }
}
