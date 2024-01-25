using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class MTIdentity
    {
        public string Name { get; set; }
    }

    public class MTIdentityUpdateModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
