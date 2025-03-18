namespace MTWireGuard.Application.Models.Requests
{
    public class CreateClientRequest
    {
        public string Name { get; set; }
        public string? Endpoint { get; set; }
        public ushort? EndpointPort { get; set; }
        public string? IPAddress { get; set; }
        public string? AllowedAddress { get; set; }
        public string? AllowedIPs { get; set; }
        public string? PresharedKey { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Interface { get; set; }
        public int? KeepAlive { get; set; }
        public bool Enabled { get; set; }
        public string? Expire { get; set; }
        public string? Password { get; set; }
        public int? Traffic { get; set; }
        public bool InheritAllowedAddress { get; set; }
        public bool InheritIP { get; set; }
        public bool InheritDNS { get; set; }
        public string? DNSAddress { get; set; }
        public string? Bandwidth { get; set; }
    }
}
