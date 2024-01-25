using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class UserActivityUpdate
    {
        public int Id { get; set; }
        public string LastHandshake { get; set; }
    }
}
