namespace MTWireGuard.Application.Models.Requests
{
    public class ImportServersRequest
    {
        public List<ImportServersItem> Servers { get; set; }
    }

    public class ImportServersItem
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public ushort ListenPort { get; set; }
        public ushort MTU { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
        public bool UseIPPool { get; set; }
        public string IPPool { get; set; }
    }
}
