using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class Log
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string Topics { get; set; }
    }
}
