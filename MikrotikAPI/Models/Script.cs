using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class Script
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("dont-require-permissions")]
        public bool DontRequiredPermissions { get; set; }
        public bool Invalid { get; set; }
        [JsonProperty("last-started")]
        public string LastStarted { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Policy { get; set; }
        [JsonProperty("run-count")]
        public int RunCount { get; set; }
        public string Source { get; set; }
    }

    public class ScriptCreateModel
    {
        [JsonProperty("dont-require-permissions")]
        public bool DontRequiredPermissions { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("policy")]
        public string Policy { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public class ScriptUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("dont-require-permissions")]
        public bool DontRequiredPermissions { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("policy")]
        public string Policy { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
