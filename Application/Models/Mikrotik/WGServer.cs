namespace MTWireGuard.Application.Models.Mikrotik
{
    public class WGServerDBModel
    {
        public int Id { get; set; }
        public string? DNSAddress { get; set; }
        public bool InheritDNS { get; set; }
        public bool UseIPPool { get; set; }
        public int? IPPoolId { get; set; }
    }

    public class WGServerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public ushort ListenPort { get; set; }
        public ushort MTU { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public bool Running { get; set; }
        public string IPAddress {  get; set; }
        public string DNSAddress { get; set; }
        public bool InheritDNS { get; set; }
        public bool UseIPPool { get; set; }
        public string IPPool { get; set; }
    }

    public class ServerCreateModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string ListenPort { get; set; }
        public string MTU { get; set; }
        public string PrivateKey { get; set; }
        public string IPAddress { get; set; }
        public bool UseIPPool { get; set; }
        public int IPPoolId { get; set; }
        public bool InheritDNS { get; set; }
        public string DNSAddress { get; set; }
    }

    public class ServerUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ushort? ListenPort { get; set; }
        public ushort? MTU { get; set; }
        public string? PrivateKey { get; set; }
        public string? IPAddress { get; set; }
        public bool UseIPPool { get; set; }
        public int? IPPoolId { get; set; }
        public bool InheritDNS { get; set; }
        public string? DNSAddress { get; set; }
    }
}
