namespace MTWireGuard.Models.Requests
{
    public class UpdateClientRequest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }
        public ushort EndpointPort { get; set; }
        public string AllowedAddress { get; set; }
        public string PresharedKey { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Interface { get; set; }
        public int KeepAlive { get; set; }
    }
}
