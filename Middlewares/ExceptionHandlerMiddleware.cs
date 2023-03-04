using MTWireGuard.Models.Mikrotik;
using System.Net;
using System.Reflection;

namespace MTWireGuard.Middlewares
{
    public class ExceptionHandlerMiddleware 
    {
        private readonly RequestDelegate _next;
        //public (HttpStatusCode code, string message) GetResponse(Exception exception);

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                // log the error
                //Logger.Error(exception, "error during executing {Context}", context.Request.Path.Value);
                Console.WriteLine(exception);
                var response = context.Response;
                response.ContentType = "application/json";

                // get the response code and message
                //var (status, message) = GetResponse(exception);
                response.StatusCode = (int)HttpStatusCode.OK;
                await response.WriteAsync(exception.Message);
            }
        }
    }

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
