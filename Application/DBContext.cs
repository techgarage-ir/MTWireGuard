using Microsoft.EntityFrameworkCore;
using MTWireGuard.Application.Models.Mikrotik;

namespace MTWireGuard.Application
{
    public class DBContext : DbContext
    {
        public DbSet<WGPeerDBModel> Users { get; set; }

        public string DbPath { get; }

        public DBContext()
        {
            DbPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "MikrotikWireguard.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

}
