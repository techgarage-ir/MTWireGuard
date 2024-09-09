using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace MTWireGuard.Application
{
    public class ExceptionHandler(Serilog.ILogger logger, IHttpContextAccessor contextAccessor) : IExceptionHandler
    {
        private readonly Serilog.ILogger logger = logger;
        private readonly IHttpContextAccessor contextAccessor = contextAccessor;

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                string exceptionType = exception.GetType().Name,
                    message = exception.Message,
                    stackTrace = exception.StackTrace,
                    source = exception.Source;
                string? innerExceptionMessage = exception.InnerException?.Message,
                    innerExceptionStackTrace = exception.InnerException?.StackTrace,
                    innerExceptionSource     = exception.InnerException?.Source;

                ExceptionHandlerContext.Message = message;
                ExceptionHandlerContext.StackTrace = stackTrace;
                ExceptionHandlerContext.Details = source;

                if (SetupValidator.IsValid)
                {
                    if (exception.InnerException == null)
                    {
                        logger.Error("{Exception}", exception);
                    }
                    else
                    {
                        logger.Error("{Exception}, {InnerException}", exception, exception.InnerException);
                    }
                }
                else
                {
                    logger.Error("Error in configuration: {Title}, {Description}", SetupValidator.Title, SetupValidator.Description);
                }

                contextAccessor.HttpContext.Response.Redirect("/Error", true);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Error In Exception Handler");
            }
            return ValueTask.FromResult(true);
        }
    }

    public static class ExceptionHandlerContext
    {
        public static string Message { get; internal set; }
        public static string StackTrace { get; internal set; }
        public static string Details { get; internal set; }
    }
}
