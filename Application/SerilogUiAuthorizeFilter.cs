using Microsoft.AspNetCore.Http;
using Serilog.Ui.Core.Interfaces;

namespace MTWireGuard.Application
{
    public class SerilogUiAuthorizeFilter(IHttpContextAccessor httpContextAccessor) : IUiAuthorizationFilter
    {
        public bool Authorize()
        {
            return httpContextAccessor.HttpContext?.User.Identity is { IsAuthenticated: true };
        }
    }
}
