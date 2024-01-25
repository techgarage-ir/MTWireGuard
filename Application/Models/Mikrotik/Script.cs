using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class ScriptViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Source { get; set; }
        public DateTime LastStarted { get; set; }
        public int RunCount { get; set; }
        public List<string> Policies { get; set; }
        public bool IsValid { get; set; }
        public bool DontRequiredPermissions { get; set; }
    }

    public class ScriptCreateModel
    {
        public string Name { get; set; }
        public List<string> Policies { get; set; }
        public string Source { get; set; }
        public bool DontRequiredPermissions { get; set; }
    }
}
