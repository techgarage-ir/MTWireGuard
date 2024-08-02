using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTWireGuard.Application.Models
{
    public class DataUsage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserID { get; set; }
        public ulong RX { get; set; }
        public ulong TX { get; set; }
        public bool UserReset { get; set; }
        public string? ResetNotes { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
