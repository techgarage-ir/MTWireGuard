using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class Job
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty(".nextid")]
        public string NextId { get; set; }
        public string Owner { get; set; }
        public string Policy { get; set; }
        public string Started { get; set; }
        public string Type { get; set; }
    }
}
