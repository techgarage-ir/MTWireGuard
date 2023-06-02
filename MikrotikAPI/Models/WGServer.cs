using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
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
