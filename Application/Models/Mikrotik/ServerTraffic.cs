namespace MTWireGuard.Application.Models.Mikrotik
{
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
