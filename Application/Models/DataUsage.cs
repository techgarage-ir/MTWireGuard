using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models
{
    public class DataUsage
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserID { get; set; }
        public int RX { get; set; }
        public int TX { get; set; }
        public bool UserReset { get; set; }
        public string? ResetNotes { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
