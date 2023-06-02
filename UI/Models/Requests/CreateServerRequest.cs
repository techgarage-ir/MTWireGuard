namespace MTWireGuard.Models.Requests
{
    public class CreateServerRequest
    {
        public string Name { get; set; }

        public ushort Port { get; set; } = 13231;

        public ushort MTU { get; set; } = 1420;

        public string PrivateKey { get; set; }

        public bool Enabled { get; set; }
    }
}
