using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Repositories;
using server.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot",
    Args = args
});

builder.WebHost.ConfigureAppConfiguration((ctx, config) =>
{
    config.AddJsonFile("appsettings.json");
    config.AddJsonFile("appsettings.development.json", optional: true);
    config.AddEnvironmentVariables();
});

builder.WebHost.ConfigureKestrel(o =>
{
    //o.ListenAnyIP(443);
    o.ConfigureEndpointDefaults(t =>
        {
            t.Protocols = HttpProtocols.Http1AndHttp2;
        });
});


// Add services to the container.
var services = builder.Services;
services.AddControllers();
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddCors(options =>
{
    options.AddPolicy("local", policy =>
    {
        policy.AllowAnyOrigin();
    });
});



builder.Services.AddDbContextFactory<WalletContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default") //builder.Configuration.GetValue<string>("ConnectionString")
        ,
        x =>
        {
            x.MigrationsAssembly("server");
        }));

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<NavContextService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<NotifyService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped(typeof(IRepo<>), typeof(Repo<>));

builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
{
    o.Level = CompressionLevel.Optimal;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
