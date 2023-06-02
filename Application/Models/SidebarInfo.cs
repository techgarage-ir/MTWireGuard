namespace MTWireGuard.Application.Models
{
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
