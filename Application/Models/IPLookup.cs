using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class IPAPIResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("isp")]
        public string? ISP { get; set; }
        [JsonProperty("org")]
        public string? ORG { get; set; }
    }
    public class IPAPIFailResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class IPLookup
    {
        public string Country { get; set; }
        public string ISP { get; set; }
    }
}
