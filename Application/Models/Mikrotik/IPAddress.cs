using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class IPAddressViewModel
    {
        public int Id { get; set; }
        public string ActualInterface { get; set; }
        public string Address { get; set; }
        public bool Enabled { get; set; }
        public bool Dynamic { get; set; }
        public string Interface { get; set; }
        public bool Valid { get; set; }
        public string Network { get; set; }
    }
}
