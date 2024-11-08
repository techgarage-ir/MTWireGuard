using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using MTWireGuard.Application.Mapper;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Services;
using Serilog;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.SqliteDataProvider.Extensions;
using Serilog.Ui.Web.Extensions;
using System.Reflection;

namespace MTWireGuard.Application
{
    public static class ApplicationServiceRegister
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Add Serilog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(Helper.LoggerConfiguration(), dispose: true);
            });

            // Add DBContext
            services.AddDbContext<DBContext>();

            // Auto Mapper Configurations
            services.AddSingleton<PeerMapping>();
            services.AddSingleton<ServerMapping>();
            services.AddSingleton<MappingProfile>();
            services.AddSingleton<RequestProfile>();
            services.AddAutoMapper(
                (provider, expression) =>
                {
                    expression.AddProfile(provider.GetService<PeerMapping>());
                    expression.AddProfile(provider.GetService<ServerMapping>());
                    expression.AddProfile(provider.GetService<MappingProfile>());
                    expression.AddProfile(provider.GetService<RequestProfile>());
                },
                new List<Assembly>());

            // Add Mikrotik API Service
            services.AddScoped<IMikrotikRepository, MTAPI>();

            // XSRF Protection
            services.AddAntiforgery(o =>
            {
                o.HeaderName = "XSRF-TOKEN";
                o.FormFieldName = "XSRF-Validation-Token";
                o.Cookie.Name = "XSRF-Validation";
            });

            // Authentication and Authorization
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = true;
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/Forbidden";
                options.Cookie.Name = "Authentication";
                options.LogoutPath = "/Logout";
            });

            services.ConfigureApplicationCookie(configure =>
            {
                configure.Cookie.Name = "MTWireguard";
            });

            services.AddAuthorization();

            // Add Razor Pages
            services.AddRazorPages().AddRazorPagesOptions(o =>
            {
                //o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                o.Conventions.AuthorizeFolder("/");
                o.Conventions.AllowAnonymousToPage("/Login");
            });

            // Add HttpContextAccessor 
            services.AddHttpContextAccessor();

            // Add Session
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add CORS
            services.AddCors();

            // Add SerilogUI
            services.AddSerilogUi(options =>
            {
                options.UseSqliteServer(setupOptions =>
                {
                    setupOptions.WithConnectionString($"Data Source={Helper.GetLogPath("logs.db")}");
                    setupOptions.WithTable("Logs");
                });

                options.AddScopedSyncAuthFilter<SerilogUiAuthorizeFilter>();
            });
        }
    }
}
