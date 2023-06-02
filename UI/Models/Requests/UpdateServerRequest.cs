namespace MTWireGuard.Models.Requests
{
    public class UpdateServerRequest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ushort Port { get; set; }

        public ushort MTU { get; set; }

        public string PrivateKey { get; set; }
    }
}
