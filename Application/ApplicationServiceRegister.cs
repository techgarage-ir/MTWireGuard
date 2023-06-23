using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.Extensions.DependencyInjection;
using MTWireGuard.Application.Mapper;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Services;
using System.Reflection;

namespace MTWireGuard.Application
{
    public static class ApplicationServiceRegister
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Add DBContext
            services.AddDbContext<DBContext>(ServiceLifetime.Singleton);

            // Add HangFire
            services.AddHangfire(config =>
            {
                config.UseSQLiteStorage(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "MikrotikWireguard.db"));
            });
            services.AddHangfireServer();

            // Auto Mapper Configurations
            services.AddSingleton<PeerMapping>();
            services.AddSingleton<ServerMapping>();
            services.AddSingleton<MappingProfile>();
            services.AddAutoMapper(
                (provider, expression) => {
                    expression.AddProfile(provider.GetService<PeerMapping>());
                    expression.AddProfile(provider.GetService<ServerMapping>());
                    expression.AddProfile(provider.GetService<MappingProfile>());
                },
                new List<Assembly>());

            // Add Mikrotik API Service
            services.AddSingleton<IMikrotikRepository, MTAPI>();

        }
    }
}
