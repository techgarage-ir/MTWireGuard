using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class IPAddress
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("actual-interface")]
        public string ActualInterface { get; set; }
        public string Address { get; set; }
        public bool Disabled { get; set; }
        public bool Dynamic { get; set; }
        public string Interface { get; set; }
        public bool Invalid { get; set; }
        public string Network { get; set; }
    }

    public class IPAddressCreateModel
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
    }

    public class IPAddressUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { set; get; }
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
