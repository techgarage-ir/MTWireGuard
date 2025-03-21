using Microsoft.EntityFrameworkCore;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Utils;

namespace MTWireGuard.Application
{
    public class DBContext : DbContext
    {
        public DbSet<WGPeerDBModel> Users { get; set; }
        public DbSet<WGServerDBModel> Servers { get; set; }
        public DbSet<DataUsage> DataUsages { get; set; }
        public DbSet<LastKnownTraffic> LastKnownTraffic { get; set; }

        public string DbPath { get; }

        public DBContext()
        {
            DbPath = Constants.DataPath("MTWireguard.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}", opt =>
            {
                opt.MigrationsAssembly("MTWireGuard.Application");
                opt.MigrationsHistoryTable("MigrationHistory");
            });
        }
    }
}
