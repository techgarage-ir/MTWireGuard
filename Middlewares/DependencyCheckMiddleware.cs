using MTWireGuard.Pages;
using MTWireGuard.Repositories;
using Razor.Templating.Core;
using System.Globalization;

namespace MTWireGuard.Middlewares
{
    public class DependencyCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMikrotikRepository API;

        public DependencyCheckMiddleware(RequestDelegate next, IMikrotikRepository mikrotik)
        {
            _next = next;
            API = mikrotik;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool Error = false;

            string? IP = Environment.GetEnvironmentVariable("MT_IP");
            string? USER = Environment.GetEnvironmentVariable("MT_USER");
            string? PASS = Environment.GetEnvironmentVariable("MT_PASS");
            string? PUBLICIP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP");

            ErrorModel errorModel = new();
            Dictionary<string, object> ViewBag = new();

            if (string.IsNullOrEmpty(IP) || string.IsNullOrEmpty(USER) || string.IsNullOrEmpty(PUBLICIP))
            {
                ViewBag["Title"] = "Environment variables are not set!";
                ViewBag["Message"] = "Please set \"MT_IP\", \"MT_USER\", \"MT_PASS\", \"MT_PUBLIC_IP\" variables in container environment.";
                Error = true;
            }
            else
            {
                if (context.Request.Path.Value == "/Login")
                {
                    await _next(context);
                    return;
                }
                try
                {
                    bool APIEnabled = await API.TryConnectAsync();
                }
                catch (Exception ex)
                {
                    ViewBag["Title"] = "Error connecting to the router api!";
                    ViewBag["Message"] = ex.Message;
                    Error = true;
                }
            }

            if (Error)
            {
                var html = await RazorTemplateEngine.RenderAsync("/Pages/Error.cshtml", errorModel, ViewBag);
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(html);
                return;
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class DependencyCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseDependencyCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DependencyCheckMiddleware>();
        }
    }
}