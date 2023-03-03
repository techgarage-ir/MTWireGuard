namespace MTWireGuard.Models
{
    public class CreationStatus
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Detail { get; set; }
        public string Message { get; set; }
        public object Item { get; set; }
    }

    public class CreationResult
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
