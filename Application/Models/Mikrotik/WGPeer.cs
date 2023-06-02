using Microsoft.EntityFrameworkCore;

namespace MTWireGuard.Application.Models.Mikrotik
{
    [PrimaryKey("Id")]
    [Index("PrivateKey", IsUnique = true)]
    [Index("PublicKey", IsUnique = true)]
    public class WGPeerDBModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }

    public class WGPeerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CurrentAddress { get; set; }
        public bool IsEnabled { get; set; }
        public string Interface { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Download { get; set; }
        public string Upload { get; set; }
        public long DownloadBytes { get; set; }
        public long UploadBytes { get; set; }
        public bool IsDifferent { get; set; }
    }

    public class UserCreateModel
    {
        public string Name { get; set; }
        public string PrivateKey { get; set; }
        public string AllowedAddress { get; set; }
        public bool Disabled { get; set; }
        public string Interface { get; set; }
        public string EndpointAddress { get; set; }
        public string EndpointPort { get; set; }
        public string PublicKey { get; set; }
        public string PresharedKey { get; set; }
        public string PersistentKeepalive { get; set; }
    }

    public class UserSyncModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }

    public class UserUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AllowedAddress { get; set; }
        public string Interface { get; set; }
        public string EndpointAddress { get; set; }
        public ushort EndpointPort { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string PresharedKey { get; set; }
        public int PersistentKeepalive { get; set; }
    }
}
