using Microsoft.AspNetCore.Antiforgery;
using MTWireGuard.Pages;
using MTWireGuard.Repositories;
using Newtonsoft.Json;
using Razor.Templating.Core;
using System.Globalization;

namespace MTWireGuard.Middlewares
{
    public class AntiForgeryMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.Value == "/Login")
            {
                await next(context);
                return;
            }
            try
            {
                var antiForgeryService = context.RequestServices.GetRequiredService<IAntiforgery>();
                var isGetRequest = string.Equals("GET", context.Request.Method, StringComparison.OrdinalIgnoreCase);
                if (!isGetRequest)
                {
                    await antiForgeryService.ValidateRequestAsync(context);
                }
            }
            catch (AntiforgeryValidationException ex)
            {
                string response = string.Empty;
                ErrorModel errorModel = new();
                Dictionary<string, object> ViewBag = new()
                {
                    ["Title"] = "XSRF token validation failed!",
                    ["Message"] = ex.Message
                };
                if (context.Request.Path.Value == "/Login")
                    response = await RazorTemplateEngine.RenderAsync("/Pages/Error.cshtml", errorModel, ViewBag);
                else
                {
                    var result = new Dictionary<string, string>()
                    {
                        {"background", "danger"},
                        {"title", ViewBag["Title"].ToString()},
                        {"body", ViewBag["Message"].ToString()},
                    };
                    response = JsonConvert.SerializeObject(result);
                }
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(response);
                return;
            }
            await next(context);
        }
    }

    public static class AntiForgeryMiddlewareExtensions
    {
        public static IApplicationBuilder UseAntiForgery(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AntiForgeryMiddleware>();
        }
    }
}
