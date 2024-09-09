using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class CountOnly
    {
        [JsonProperty("ret")]
        public uint Ret { get; set; }
    }
}
