using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class IPPool
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ranges { get; set; }
        [JsonProperty("next-pool")]
        public string NextPool { get; set; }
    }

    public class IPPoolCreateModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranges")]
        public string Ranges { get; set; }
        [JsonProperty("next-pool")]
        public string? NextPool { get; set; }
    }
    public class IPPoolUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranges")]
        public string Ranges { get; set; }
        [JsonProperty("next-pool")]
        public string NextPool { get; set; }
    }
}
