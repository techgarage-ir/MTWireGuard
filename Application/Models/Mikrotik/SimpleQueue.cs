namespace MTWireGuard.Application.Models.Mikrotik
{
    public class SimpleQueueViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public ulong MaxLimitUpload { get; set; }
        public ulong MaxLimitDownload { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public ulong TotalBytes { get; set; }
    }
}
