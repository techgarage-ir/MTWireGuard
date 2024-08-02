using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class LastKnownTraffic
    {
        [Key]
        public int UserID { get; set; }
        [DefaultValue(0)]
        public ulong RX { get; set; }
        [DefaultValue(0)]
        public ulong TX { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
