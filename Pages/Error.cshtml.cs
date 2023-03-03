using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace MTWireGuard.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public string? ExceptionMessage { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public ErrorModel()
        {
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            ViewData["Title"] = exceptionHandlerPathFeature.Error.Message;
            ViewData["Message"] = exceptionHandlerPathFeature.Error.Source;
        }
    }
}