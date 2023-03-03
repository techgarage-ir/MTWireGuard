using Newtonsoft.Json;

namespace MTWireGuard.Models.Mikrotik
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

    public class JobViewModel
    {
        public short Id { get; set; }
        public short NextId { get; set; }
        public string Owner { get; set; }
        public List<string> Policies { get; set; }
        public string Started { get; set; }
        public string Type { get; set; }
    }
}
