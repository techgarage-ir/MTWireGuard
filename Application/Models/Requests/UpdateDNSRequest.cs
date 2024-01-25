using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Requests
{
    public class UpdateDNSRequest
    {
        public List<string> Servers { get; set; }
    }
}
