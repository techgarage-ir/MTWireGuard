using Newtonsoft.Json;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class JobViewModel
    {
        public short Id { get; set; }
        public short NextId { get; set; }
        public string Owner { get; set; }
        public List<string> Policies { get; set; }
        public string Started { get; set; }
        public string Type { get; set; }
    }
}
