using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MTWireGuard.Models.Mikrotik;

namespace MTWireGuard
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
