using MTWireGuard.Application;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Middlewares
{
    public class ClientReportingMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, IMikrotikRepository api)
        {
            if ((context.Request.Path.Value == "/" || context.Request.Path.Value == "/Index") && (!context.User.Identity.IsAuthenticated))
            {
                var ip = context.Connection.RemoteIpAddress;
                var users = await api.GetUsersAsync();
                var user = users.Find(x => x.IPAddress == $"{ip}/32");
                if (user != null)
                {
                    context.Session.Set("user", user);
                    context.Response.Redirect("/Client", true);
                }
            }

            await next(context);
        }
    }


    public static class ClientReportingMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientReporting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientReportingMiddleware>();
        }
    }
}
