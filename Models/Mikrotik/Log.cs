using Newtonsoft.Json;

namespace MTWireGuard.Models.Mikrotik
{
    public class Log
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string Topics { get; set; }
    }

    public class LogViewModel
    {
        public ulong Id { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public List<string> Topics { get; set; }
    }
}
