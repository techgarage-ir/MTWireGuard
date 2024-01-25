using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikAPI.Models
{
    public class DNS
    {
        [JsonProperty("allow-remote-requests")]
        public bool AllowRemoteRequests { get; set; }

        [JsonProperty("cache-max-ttl")]
        public string CacheMaxTtl { get; set; }

        [JsonProperty("cache-size")]
        public int CacheSize { get; set; }

        [JsonProperty("cache-used")]
        public int CacheUsed { get; set; }

        [JsonProperty("doh-max-concurrent-queries")]
        public int DohMaxConcurrentQueries { get; set; }

        [JsonProperty("doh-max-server-connections")]
        public int DohMaxServerConnections { get; set; }

        [JsonProperty("doh-timeout")]
        public string DohTimeout { get; set; }

        [JsonProperty("dynamic-servers")]
        public string DynamicServers { get; set; }

        [JsonProperty("max-concurrent-queries")]
        public int MaxConcurrentQueries { get; set; }

        [JsonProperty("max-concurrent-tcp-sessions")]
        public int MaxConcurrentTcpSessions { get; set; }

        [JsonProperty("max-udp-packet-size")]
        public int MaxUdpPacketSize { get; set; }

        [JsonProperty("query-server-timeout")]
        public string QueryServerTimeout { get; set; }

        [JsonProperty("query-total-timeout")]
        public string QueryTotalTimeout { get; set; }

        [JsonProperty("servers")]
        public string Servers { get; set; }

        [JsonProperty("use-doh-server")]
        public string UseDohServer { get; set; }

        [JsonProperty("verify-doh-cert")]
        public bool VerifyDohCert { get; set; }
    }

    public class MTDNSUpdateModel
    {
        [JsonProperty("servers")]
        public string Servers { get; set; }
    }
}
