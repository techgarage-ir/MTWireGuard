namespace MTWireGuard.Application.Models.Mikrotik
{
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
}
