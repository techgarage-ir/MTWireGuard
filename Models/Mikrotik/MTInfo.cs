using Newtonsoft.Json;

namespace MTWireGuard.Models.Mikrotik
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

    public class MTInfoViewModel
    {
        public string Architecture { get; set; }
        public string BoardName { get; set; }
        public string Platform { get; set; }
        public string CPU { get; set; }
        public byte CPUCount { get; set; }
        public short CPUFrequency { get; set; }
        public byte CPULoad { get; set; }
        public string TotalHDD { get; set; }
        public string UsedHDD { get; set; }
        public string FreeHDD { get; set; }
        public long TotalHDDBytes { get; set; }
        public long UsedHDDBytes { get; set; }
        public long FreeHDDBytes { get; set; }
        public byte FreeHDDPercentage { get; set; }
        public string TotalRAM { get; set; }
        public string UsedRAM { get; set; }
        public string FreeRAM { get; set; }
        public long TotalRAMBytes { get; set; }
        public long UsedRAMBytes { get; set; }
        public long FreeRAMBytes { get; set; }
        public byte FreeRAMPercentage { get; set; }
        public string UPTime { get; set; }
        public string Version { get; set; }
    }

    public class SidebarInfo
    {
        public int RAMUsedPercentage { get; set; }
        public int CPUUsedPercentage { get; set; }
        public int HDDUsedPercentage { get; set; }
        public string RAMUsed { get; set; }
        public string HDDUsed { get; set; }
        public string TotalRAM { get; set; }
        public string TotalHDD { get; set; }
        public string RAMBgColor { get; set; }
        public string CPUBgColor { get; set; }
        public string HDDBgColor { get; set; }
    }
}
