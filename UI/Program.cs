using Microsoft.AspNetCore.Authentication.Cookies;
using MTWireGuard.Middlewares;
using MTWireGuard.Application;
using Microsoft.Extensions.Caching.Memory;
using MTWireGuard.Application.MinimalAPI;
using Serilog;
using Serilog.Exceptions.Core;
using Serilog.Exceptions;
using System.Configuration;
using Serilog.Ui.Web;
using Serilog.Ui.Web.Authorization;

internal class Program
{
    public static bool isValid {  get; private set; }
    public static string validationMessage { get; private set; }

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddExceptionHandler<ExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddApplicationServices();

        builder.Host.UseSerilog(Helper.LoggerConfiguration());

        var app = builder.Build();

        app.UseHttpsRedirection();

        var serviceScope = app.Services.CreateScope().ServiceProvider;

        // Validate Prerequisite
        var validator = new SetupValidator(serviceScope);
        isValid = await validator.Validate();

        if (!app.Environment.IsDevelopment())
        {
            app.UseStaticFiles();
            app.UseHsts();
        }
        else
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Append("Expires", "-1");
                }
            });

        app.UseExceptionHandler();
        app.UseClientReporting();
        //app.UseAntiForgery();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSession();

        app.MapRazorPages();

        app.
            MapGroup("/api/").
            MapGeneralApi();

        app.UseCors(options =>
        {
            options.AllowAnyHeader();
            options.AllowAnyMethod();
            options.AllowAnyOrigin();
        });

        app.UseSerilogRequestLogging();

        app.UseSerilogUi(options =>
        {
            options.RoutePrefix = "Debug";
            options.InjectStylesheet("/assets/lib/boxicons/css/boxicons.min.css");
            options.InjectStylesheet("/assets/css/serilogui.css");
            options.InjectJavascript("/assets/js/serilogui.js");
            options.Authorization.AuthenticationType = AuthenticationType.Jwt;
            options.Authorization.Filters =
            [
                new SerilogUiAuthorizeFilter()
            ];
        });

        app.Run();
    }
}
