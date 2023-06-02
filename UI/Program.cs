using Microsoft.AspNetCore.Authentication.Cookies;
using MTWireGuard.Middlewares;
using MTWireGuard.Application;
using MTWireGuard.Mapper;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAutoMapper(typeof(RequestProfile));

builder.Services.AddApplicationServices();

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

app.UseHttpsRedirection();

// Ensure Database Exists
var serviceScope = app.Services.CreateScope().ServiceProvider;
serviceScope.GetService<DBContext>().Database.EnsureCreated();

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