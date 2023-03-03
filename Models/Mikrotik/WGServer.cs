using Newtonsoft.Json;
using System.ComponentModel;

namespace MTWireGuard.Models.Mikrotik
{
    public class WGServer
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("listen-port")]
        public string ListenPort { get; set; }
        [JsonProperty("mtu")]
        public string MTU { get; set; }
        [JsonProperty("private-key")]
        public string PrivateKey { get; set; }
        [JsonProperty("public-key")]
        public string PublicKey { get; set; }
        [JsonProperty("running")]
        public bool Running { get; set; }
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
    }

    public class ServerCreateModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string ListenPort { get; set; }
        public string MTU { get; set; }
        public string PrivateKey { get; set; }
    }

    public class ServerUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ushort ListenPort { get; set; }
        public ushort MTU { get; set; }
        public string PrivateKey { get; set; }
    }

    public class WGServerCreateModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("disabled"), DefaultValue(false)]
        public bool Disabled { get; set; }
        [JsonProperty("listen-port")]
        public ushort ListenPort { get; set; }
        [JsonProperty("mtu")]
        public ushort MTU { get; set; }
        [JsonProperty("private-key")]
        public string PrivateKey { get; set; }
    }

    public class WGServerUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name"), DefaultValue("")]
        public string Name { get; set; }
        [JsonProperty("listen-port"), DefaultValue(0)]
        public ushort ListenPort { get; set; }
        [JsonProperty("mtu"), DefaultValue(0)]
        public ushort MTU { get; set; }
        [JsonProperty("private-key"), DefaultValue("")]
        public string PrivateKey { get; set; }
    }
}
