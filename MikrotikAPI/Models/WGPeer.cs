using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class WGPeer
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
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
        [JsonProperty("private-key")]
        public string PrivateKey { get; set; }
        [JsonProperty("public-key")]
        public string PublicKey { get; set; }
        [JsonProperty("client-address")]
        public string ClientAddress { get; set; }
        [JsonProperty("client-dns")]
        public string DNSAddress { get; set; }
        [JsonProperty("rx")]
        public string RX { get; set; }
        [JsonProperty("tx")]
        public string TX { get; set; }
    }

    public class WGPeerLastHandshake
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("last-handshake")]
        public string? LastHandshake { get; set; }
    }

    public class WGPeerCreateModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("allowed-address")]
        public string AllowedAddress { get; set; }
        [JsonProperty("disabled"), DefaultValue(false)]
        public bool Disabled { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("endpoint-address"), DefaultValue("")]
        public string EndpointAddress { get; set; }
        [JsonProperty("endpoint-port"), DefaultValue("")]
        public string EndpointPort { get; set; }
        [JsonProperty("private-key"), DefaultValue("")]
        public string PrivateKey { get; set; }
        [JsonProperty("public-key"), DefaultValue("")]
        public string PublicKey { get; set; }
        [JsonProperty("preshared-key"), DefaultValue("")]
        public string PresharedKey { get; set; }
        [JsonProperty("client-address")]
        public string ClientAddress { get; set; }
        [JsonProperty("client-dns"), DefaultValue("")]
        public string DNSAddress { get; set; }
        [JsonProperty("persistent-keepalive"), DefaultValue("")]
        public string PersistentKeepalive { get; set; }
    }

    public class WGPeerUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name"), DefaultValue("")]
        public string Name { get; set; }
        [JsonProperty("allowed-address"), DefaultValue("")]
        public string AllowedAddress { get; set; }
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("endpoint-address"), DefaultValue("")]
        public string EndpointAddress { get; set; }
        [JsonProperty("endpoint-port"), DefaultValue(0)]
        public ushort EndpointPort { get; set; }
        [JsonProperty("private-key"), DefaultValue("")]
        public string PrivateKey { get; set; }
        [JsonProperty("public-key"), DefaultValue("")]
        public string PublicKey { get; set; }
        [JsonProperty("preshared-key"), DefaultValue("")]
        public string PresharedKey { get; set; }
        [JsonProperty("client-address")]
        public string ClientAddress { get; set; }
        [JsonProperty("client-dns"), DefaultValue("")]
        public string DNSAddress { get; set; }
        [JsonProperty("persistent-keepalive"), DefaultValue(0)]
        public int PersistentKeepalive { get; set; }
    }
}
