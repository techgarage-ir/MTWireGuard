using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class SimpleQueue
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("bytes")]
        public string Bytes { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("max-limit")]
        public string MaxLimit { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
        [JsonProperty("total-bytes")]
        public string TotalBytes { get; set; }
    }
    public class SimpleQueueCreateModel
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("max-limit")]
        public string MaxLimit { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
    }
    public class SimpleQueueUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("max-limit")]
        public string MaxLimit { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
    }
}
