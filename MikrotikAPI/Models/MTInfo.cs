using Newtonsoft.Json;

namespace MikrotikAPI.Models
{
    public class MTInfo
    {
        [JsonProperty("architecture-name")]
        public string ArchitectureName { get; set; }

        [JsonProperty("board-name")]
        public string BoardName { get; set; }

        [JsonProperty("build-time")]
        public string BuildTime { get; set; }
        public string CPU { get; set; }

        [JsonProperty("cpu-count")]
        public string CPUCount { get; set; }

        [JsonProperty("cpu-frequency")]
        public string CPUFrequency { get; set; }

        [JsonProperty("cpu-load")]
        public string CPULoad { get; set; }

        [JsonProperty("free-hdd-space")]
        public string FreeHDDSpace { get; set; }

        [JsonProperty("free-memory")]
        public string FreeMemory { get; set; }
        public string Platform { get; set; }

        [JsonProperty("total-hdd-space")]
        public string TotalHDDSpace { get; set; }

        [JsonProperty("total-memory")]
        public string TotalMemory { get; set; }
        public string Uptime { get; set; }
        public string Version { get; set; }

        [JsonProperty("write-sect-since-reboot")]
        public string WriteSectSinceReboot { get; set; }

        [JsonProperty("write-sect-total")]
        public string WriteSectTotal { get; set; }
    }
}
