using Microsoft.EntityFrameworkCore;

namespace MTWireGuard.Application.Models.Mikrotik
{
    [PrimaryKey("Id")]
    [Index("Id", IsUnique = true)]

    public class WGPeerDBModel
    {
        public int Id { get; set; }
        public string AllowedIPs { get; set; }
        public ulong RX { get; set; }
        public ulong TX { get; set; }
        public int TrafficLimit { get; set; }
        public bool InheritDNS { get; set; }
        public bool InheritIP { get; set; }
    }

    public class WGPeerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AllowedAddress { get; set; }
        public string AllowedIPs { get; set; }
        public string CurrentAddress { get; set; }
        public bool IsEnabled { get; set; }
        public string Interface { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Download { get; set; }
        public string Upload { get; set; }
        public ulong DownloadBytes { get; set; }
        public ulong UploadBytes { get; set; }
        public string Expire { get; set; }
        public int Traffic { get; set; }
        public ulong TrafficUsed { get; set; }
        public string LastHandshake { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
        public bool InheritDNS { get; set; }
        public bool InheritIP { get; set; }
        public string Bandwidth { get; set; }
    }

    public class UserCreateModel
    {
        public string Name { get; set; }
        public string PrivateKey { get; set; }
        public string AllowedAddress { get; set; }
        public string AllowedIPs { get; set; }
        public bool Disabled { get; set; }
        public string Interface { get; set; }
        public string EndpointAddress { get; set; }
        public string EndpointPort { get; set; }
        public string PublicKey { get; set; }
        public string PresharedKey { get; set; }
        public string PersistentKeepalive { get; set; }
        public DateTime? Expire { get; set; } = null;
        public int Traffic { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
        public bool InheritAllowedAddress { get; set; }
        public bool InheritDNS { get; set; }
        public bool InheritIP { get; set; }
        public string? Bandwidth { get; set; }
    }

    public class UserUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AllowedAddress { get; set; }
        public string AllowedIPs { get; set; }
        public string Interface { get; set; }
        public string EndpointAddress { get; set; }
        public ushort EndpointPort { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string PresharedKey { get; set; }
        public int PersistentKeepalive { get; set; }
        public DateTime Expire { get; set; }
        public int Traffic { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
        public bool InheritAllowedAddress { get; set; }
        public bool InheritDNS { get; set; }
        public bool InheritIP { get; set; }
        public string Bandwidth { get; set; }
    }

    public class WGPeerLastHandshakeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan LastHandshake { get; set; }
    }

    public class UserImportModel
    {
        public string Name { get; set; }
        public string AllowedAddress { get; set; }
        public string AllowedIPs { get; set; }
        public string CurrentAddress { get; set; }
        public bool Enabled { get; set; }
        public string Interface { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public ulong DownloadBytes { get; set; }
        public ulong UploadBytes { get; set; }
        public string Expire { get; set; }
        public int Traffic { get; set; }
        public string IPAddress { get; set; }
        public string DNSAddress { get; set; }
        public string Bandwidth { get; set; }
    }
}
