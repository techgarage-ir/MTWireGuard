namespace MTWireGuard.Application.Models.Requests
{
    public class ImportUsersRequest
    {
        public List<ImportUsersItem> Users { get; set; }
    }

    public class ImportUsersItem
    {
        public string Name { get; set; }
        public string AllowedAddress { get; set; }
        public string AllowedIPs { get; set; }
        public string CurrentAddress { get; set; }
        public bool IsEnabled { get; set; }
        public string Interface { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public ulong DownloadBytes { get; set; }
        public ulong UploadBytes { get; set; }
        public string Expire { get; set; }
        public int Traffic { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
    }
}
