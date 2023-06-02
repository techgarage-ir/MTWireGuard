using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class ServerTraffic
    {
        public string Name { get; set; }
        public string Type { get; set; }
        [JsonProperty("tx-byte")]
        public string TX { get; set; }
        [JsonProperty("rx-byte")]
        public string RX { get; set; }
    }
}
