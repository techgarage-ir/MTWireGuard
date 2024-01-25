namespace MTWireGuard.Application.Models.Requests
{
    public class CreateScriptRequest
    {
        public string Name { get; set; }
        public List<string> Policy { get; set; }
        public string Source { get; set; }
        public bool DontRequiredPermissions { get; set; }
    }
}
