using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MTWireGuard.Models.Mikrotik
{
    public class WGPeer
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("allowed-address")]
        public string AllowedAddress { get; set; }
        [JsonProperty("current-endpoint-address")]
        public string CurrentEndpointAddress { get; set; }
        [JsonProperty("current-endpoint-port")]
        public string CurrentEndpointPort { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("endpoint-address")]
        public string EndpointAddress { get; set; }
        [JsonProperty("endpoint-port")]
        public string EndpointPort { get; set; }
        [JsonProperty("public-key")]
        public string PublicKey { get; set; }
        [JsonProperty("rx")]
        public string RX { get; set; }
        [JsonProperty("tx")]
        public string TX { get; set; }
    }

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

    public class WGPeerCreateModel
    {
        [JsonProperty("allowed-address")]
        public string AllowedAddress { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("endpoint-address"), DefaultValue("")]
        public string EndpointAddress { get; set; }
        [JsonProperty("endpoint-port"), DefaultValue("")]
        public string EndpointPort { get; set; }
        [JsonProperty("public-key")]
        public string PublicKey { get; set; }
        [JsonProperty("preshared-key"), DefaultValue("")]
        public string PresharedKey { get; set; }
        [JsonProperty("persistent-keepalive"), DefaultValue("")]
        public string PersistentKeepalive { get; set; }
    }

    public class WGPeerUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("allowed-address"), DefaultValue("")]
        public string AllowedAddress { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("endpoint-address"), DefaultValue("")]
        public string EndpointAddress { get; set; }
        [JsonProperty("endpoint-port"), DefaultValue(0)]
        public ushort EndpointPort { get; set; }
        [JsonProperty("public-key"), DefaultValue("")]
        public string PublicKey { get; set; }
        [JsonProperty("preshared-key"), DefaultValue("")]
        public string PresharedKey { get; set; }
        [JsonProperty("persistent-keepalive"), DefaultValue(0)]
        public int PersistentKeepalive { get; set; }
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
