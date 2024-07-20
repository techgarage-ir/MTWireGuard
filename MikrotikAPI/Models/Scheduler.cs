using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class Scheduler
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        [JsonProperty("start-date")]
        public string StartDate { get; set; }
        [JsonProperty("start-time")]
        public string StartTime { get; set; }
        public string Interval { get; set; }
        public string Policy { get; set; }
        [JsonProperty("run-count")]
        public int RunCount { get; set; }
        [JsonProperty("next-run")]
        public string NextRun { get; set; }
        [JsonProperty("on-event")]
        public string OnEvent { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        public bool Disabled { get; set; }
    }
    public class SchedulerCreateModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("start-date")]
        public string StartDate { get; set; }
        [JsonProperty("start-time")]
        public string StartTime { get; set; }
        [JsonProperty("interval")]
        public string Interval { get; set; }
        [JsonProperty("policy")]
        public string Policy { get; set; }
        [JsonProperty("on-event")]
        public string OnEvent { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
    public class SchedulerUpdateModel
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("start-date")]
        public string StartDate { get; set; }
        [JsonProperty("start-time")]
        public string StartTime { get; set; }
        [JsonProperty("interval")]
        public string Interval { get; set; }
        [JsonProperty("policy")]
        public string Policy { get; set; }
        [JsonProperty("on-event")]
        public string OnEvent { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
