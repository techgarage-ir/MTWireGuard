namespace MTWireGuard.Application.Models.Requests
{
    public class UpdateServerRequest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ushort? Port { get; set; }

        public ushort? MTU { get; set; }

        public string? PrivateKey { get; set; }

        public string? IPAddress { get; set; }

        public bool UseIPPool { get; set; }

        public int? IPPoolId { get; set; }

        public bool InheritDNS { get; set; }

        public string? DNSAddress { get; set; }
    }
}
