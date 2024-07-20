using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    //details = JsonConvert.SerializeObject(exception)!;
                    details = exception.Source;

                ExceptionHandlerContext.Message = message;
                ExceptionHandlerContext.StackTrace = stackTrace;
                ExceptionHandlerContext.Details = details;

                if (SetupValidator.IsValid)
                {
                    logger.Error(exception, "Unhandled error");
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
