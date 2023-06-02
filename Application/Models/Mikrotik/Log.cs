namespace MTWireGuard.Application.Models.Mikrotik
{
    public class LogViewModel
    {
        public ulong Id { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public List<string> Topics { get; set; }
    }
}
