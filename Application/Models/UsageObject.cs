using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class UsageObject
    {
        public string Id { get; set; }
        public int RX { get; set; }
        public int TX { get; set; }
    }
}
