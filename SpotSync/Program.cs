using Havit.Blazor.Components.Web;
using SpotSync.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpotSync;
using SpotSync.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHxServices();

builder.Services.AddDbContext<SpotSyncContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
});
builder.Services.AddScoped<SyncService, SyncService>();
builder.Services.AddScoped<AccountService, AccountService>();
builder.Services.AddScoped<SpotifyService, SpotifyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
