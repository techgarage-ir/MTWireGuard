using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.MinimalAPI
{
    internal class AuthController
    {
        public static async Task<Results<SignInHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> Login([FromBody] LoginRequest login)
        {
            try
            {
                string MT_IP = Environment.GetEnvironmentVariable("MT_IP");
                string MT_USER = Environment.GetEnvironmentVariable("MT_USER");
                string MT_PASS = Environment.GetEnvironmentVariable("MT_PASS") ?? "";

                if (login.Username == MT_USER && login.Password == MT_PASS)
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
                        new(ClaimTypes.Role, "Administrator"),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true
                    };

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    return TypedResults.SignIn(claimsPrincipal, authProperties, CookieAuthenticationDefaults.AuthenticationScheme);
                }
                else
                {
                    return TypedResults.Unauthorized();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return TypedResults.Problem(
                    detail: ex.Message,
                    type: ex.GetType().Name);
            }
        }

        public static async Task<SignOutHttpResult> Logout(
            [FromServices] IMikrotikRepository API,
            HttpContext context)
        {
            // Clear the existing external cookie
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var sessionId = await API.GetCurrentSessionID();
            var kill = await API.KillJob(sessionId);
            return TypedResults.SignOut();
        }
    }
}
