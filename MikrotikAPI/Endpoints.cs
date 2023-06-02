namespace MikrotikAPI
{
    public static class Endpoints
    {
        public const string Log = "log";
        public const string Wireguard = "interface/wireguard";
        public const string Interface = "interface";
        public const string WireguardPeers = "interface/wireguard/peers";
        public const string SystemResource = "system/resource";
        public const string SystemIdentity = "system/identity";
        public const string ActiveUsers = "user/active";
        public const string Jobs = "system/script/job";
        public const string MonitorTraffic = "interface/monitor-traffic";
        public static string Empty => string.Empty;
    }

    public enum RequestMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }
}
