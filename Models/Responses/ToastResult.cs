using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MTWireGuard.Models.Responses
{
    public class ToastResult : IActionResult
    {
        private class Toast
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public string Background { get; set; }
        }

        private readonly Toast _result;

        public ToastResult(string title, string body, string background)
        {
            _result = new()
            {
                Title = title,
                Body = body,
                Background = background
            };
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(_result);

            await objectResult.ExecuteResultAsync(context);
        }
    }
}
