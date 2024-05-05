using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotSync.Components;
using SpotSync;
using SpotSync.Components.Account;
using SpotSync.Data;
using static SpotifyAPI.Web.Scopes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddSingleton(SpotifyClientConfig.CreateDefault());
builder.Services.AddScoped<SpotifyClientBuilder>();

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("Spotify", policy =>
	{
		policy.AuthenticationSchemes.Add("Spotify");
		policy.RequireAuthenticatedUser();
	});
});
builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	})
	.AddSpotify(options =>
	{
		options.ClientId = builder.Configuration["SPOTIFY_CLIENT_ID"] ?? throw new InvalidOperationException("SPOTIFY_CLIENT_ID not found.");
		options.ClientSecret = builder.Configuration["SPOTIFY_CLIENT_SECRET"] ?? throw new InvalidOperationException("SPOTIFY_CLIENT_SECRET not found.");
		options.CallbackPath = "/Auth/callback";
		options.SaveTokens = true;

		var scopes = new List<string> {
			UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative, PlaylistModifyPrivate, PlaylistModifyPublic, UserLibraryRead
		  };
		options.Scope.Add(string.Join(",", scopes));

		//options.ClaimActions.MapAll();

		options.ClaimActions.MapJsonKey(SpotifyClaimTypes.Product, "product", "string");
		//options.ClaimActions.MapJsonKey("urn:spotify:product", "product", "string");
		options.ClaimActions.MapJsonSubKey(SpotifyClaimTypes.ProfilePicture, "images", "url", "string");

		options.Events.OnCreatingTicket = ctx =>
		{
			var tokens = ctx.Properties.GetTokens();
			ctx.Properties.StoreTokens(tokens);

			//TODO add save tokens to DB
			return Task.CompletedTask;
		};
	})
	.AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
	{
		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
		options.SignIn.RequireConfirmedPhoneNumber = false;
		options.User.RequireUniqueEmail = true;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddSignInManager()
	.AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
