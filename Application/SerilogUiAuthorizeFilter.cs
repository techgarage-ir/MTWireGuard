using Microsoft.AspNetCore.Http;
using Serilog.Ui.Web.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application
{
    public class SerilogUiAuthorizeFilter : IUiAuthorizationFilter
    {
        public bool Authorize(HttpContext httpContext)
        {
            return httpContext.User.Identity is { IsAuthenticated: true };
        }
    }
}
