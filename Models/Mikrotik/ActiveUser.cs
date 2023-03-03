using Newtonsoft.Json;

namespace MTWireGuard.Models.Mikrotik
{
    public class ActiveUser
    {
        [JsonProperty(".id")]
        public string Id { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public bool Radius { get; set; }
        public string Via { get; set; }
        public string When { get; set; }
    }

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
