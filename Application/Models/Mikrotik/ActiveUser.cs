using Newtonsoft.Json;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class ActiveUserViewModel
    {
        public short Id { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public bool Radius { get; set; }
        public string Via { get; set; }
        public string LoggedIn { get; set; }
    }
}
