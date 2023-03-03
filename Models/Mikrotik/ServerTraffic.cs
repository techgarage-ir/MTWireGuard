using Newtonsoft.Json;

namespace MTWireGuard.Models.Mikrotik
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

    public class ServerTrafficViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Upload { get; set; }
        public string Download { get; set; }
        public long UploadBytes { get; set; }
        public long DownloadBytes { get; set; }
    }
}
