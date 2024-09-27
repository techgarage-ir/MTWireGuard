using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class WGServerStatistics
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Running { get; set; }
    }

    public class WGUserStatistics
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Online { get; set; }
    }
}
