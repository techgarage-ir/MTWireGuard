using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class IPPoolViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Ranges { get; set; }
        public string NextPool { get; set; }
    }

    public class PoolCreateModel
    {
        public string Name { get; set; }
        public string? Next { get; set; }
        public string Ranges { get; set; }
    }

    public class PoolUpdateModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Next { get; set; }
        public string Ranges { get; set; }
    }
}
