using MTWireGuard.Application;
using MTWireGuard.Application.MinimalAPI;
using MTWireGuard.Middlewares;
using Serilog;
using Serilog.Ui.Web.Extensions;

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
            options.HideSerilogUiBrand();
            options.InjectJavascript("/assets/js/serilogui.js");
            options.WithRoutePrefix("Debug");
            options.WithAuthenticationType(Serilog.Ui.Web.Models.AuthenticationType.Custom);
            options.EnableAuthorizationOnAppRoutes();
        });

        app.Run();
    }
}
