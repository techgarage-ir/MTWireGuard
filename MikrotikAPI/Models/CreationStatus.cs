namespace MikrotikAPI.Models
{
    public class CreationStatus
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Detail { get; set; }
        public string Message { get; set; }
        public object Item { get; set; }
    }
}
