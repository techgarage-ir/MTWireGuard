using Microsoft.AspNetCore.Mvc;
using MTWireGuard.Application;
using Newtonsoft.Json;

namespace MTWireGuard.Middlewares
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string exceptionType = exception.GetType().Name,
                message = exception.Message,
                stackTrace = exception.StackTrace,
                details = JsonConvert.SerializeObject(exception)!;

            var viewResult = new ViewResult
            {
                ViewName = "Error",
                StatusCode = 500
            };
            viewResult.ViewData["Title"] = message;
            viewResult.ViewData["Message"] = stackTrace;
            viewResult.ViewData["Details"] = details;

            var html = viewResult.ToHtml(context);

            return context.Response.WriteAsync(html);
        }
    }


    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
