using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace MTWireGuard.Models.Mikrotik
{
    public class EtherIP
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Address { get; set; }
    }
}
