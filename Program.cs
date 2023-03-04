using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MTWireGuard;
using MTWireGuard.Mapper;
using MTWireGuard.Middlewares;
using MTWireGuard.Repositories;
using MTWireGuard.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
using DBContext context = new();

// Add services to the container.
builder.Services.AddRazorPages().AddRazorPagesOptions(o =>
{
    //o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Login");
});

builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "XSRF-TOKEN";
    o.FormFieldName = "XSRF-Validation-Token";
    o.Cookie.Name = "XSRF-Validation";
});

// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
    mc.AddProfile(new PeerMapping(context));
    mc.AddProfile(new ServerMapping());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddSingleton(context);

builder.Services.AddSingleton<IMikrotikRepository, MTAPI>();

//builder.Services.AddScoped<AntiForgeryMiddleware>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Forbidden";
    options.Cookie.Name = "Authentication";
});

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("Administrator", authBuilder =>
        {
            authBuilder.RequireRole("Administrator");
        });
});

builder.Services.ConfigureApplicationCookie(configure =>
{
    configure.Cookie.Name = "MTWireguard";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

context.Database.EnsureCreated();

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
    app.UseStaticFiles();
else
    app.UseStaticFiles(new StaticFileOptions()
    {
        OnPrepareResponse = context =>
        {
            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            context.Context.Response.Headers.Add("Expires", "-1");
        }
    });

app.UseDependencyCheck();
//app.UseExceptionHandling();
//app.UseAntiForgery();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();