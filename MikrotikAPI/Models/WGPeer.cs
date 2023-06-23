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
}
