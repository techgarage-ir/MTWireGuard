using Microsoft.AspNetCore.Authentication.Cookies;
using MTWireGuard.Middlewares;
using MTWireGuard.Application;
using Microsoft.Extensions.Caching.Memory;
using MTWireGuard.Application.MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var serviceScope = app.Services.CreateScope().ServiceProvider;

// Validate Prerequisite
var validator = new SetupValidator(serviceScope);
await validator.Validate();

if (!app.Environment.IsDevelopment())
    app.UseStaticFiles();
else
    app.UseStaticFiles(new StaticFileOptions()
    {
        OnPrepareResponse = context =>
        {
            context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            context.Context.Response.Headers.Append("Expires", "-1");
        }
    });

app.UseDependencyCheck();
app.UseClientReporting();
app.UseExceptionHandling();
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

app.Run();
