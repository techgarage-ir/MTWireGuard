namespace MTWireGuard.Models.Requests
{
    public class SyncUserRequest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
