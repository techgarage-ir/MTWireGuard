using Newtonsoft.Json;

namespace MTWireGuard.Models
{
    public class WGEnability
    {
        [JsonProperty(".id")]
        public string ID { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }
}
