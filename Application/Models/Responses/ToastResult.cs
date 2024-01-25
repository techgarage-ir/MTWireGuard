using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MTWireGuard.Application.Models.Models.Responses
{
    public class ToastResult : IActionResult
    {
        private readonly ToastMessage _result;

        public ToastResult(ToastMessage message)
        {
            _result = message;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(_result);

            await objectResult.ExecuteResultAsync(context);
        }
    }
}
