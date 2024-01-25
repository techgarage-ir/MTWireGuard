using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Requests
{
    public class CreatePoolRequest
    {
        public string Name { get; set; }
        public string? Next {  get; set; }
        public List<string> Ranges { get; set; }
    }
}
