using System.ComponentModel;

namespace MTWireGuard.Application.Models.Requests
{
    public class CreateServerRequest
    {
        public string? Name { get; set; }

        [DefaultValue(13231)]
        public ushort? Port { get; set; }

        [DefaultValue(1420)]
        public ushort? MTU { get; set; }

        public string? PrivateKey { get; set; }

        public bool Enabled { get; set; }

        public string IPAddress { get; set; }

        public bool UseIPPool { get; set; }

        public int? IPPoolId { get; set; }

        public bool InheritDNS { get; set; }

        public string? DNSAddress { get; set; }
    }
}
