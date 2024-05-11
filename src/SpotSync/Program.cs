using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotSync.Components;
using SpotSync.Components.Account;
using SpotSync.Data;
using static SpotifyAPI.Web.Scopes;
using BytexDigital.Blazor.Components.CookieConsent;
using SpotSync.Components.Cookie;
using SpotSync.Services;
using SpotSync.Interfaces;
using SpotSync;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
//builder.Services.AddSingleton(SpotifyClientConfig.CreateDefault());

builder.Services.Configure<SpotifyOptions>(o =>
{
	o.SpotifyClientId = builder.Configuration["SPOTIFY_CLIENT_ID"] ?? throw new InvalidOperationException("SPOTIFY_CLIENT_ID not found.");
    o.SpotifyClientSecret = builder.Configuration["SPOTIFY_CLIENT_SECRET"] ?? throw new InvalidOperationException("SPOTIFY_CLIENT_SECRET not found.");
});

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
		options.ClientId = builder.Configuration[SpotifyOptions.SpotifyClientIdName] ?? throw new InvalidOperationException($"{SpotifyOptions.SpotifyClientIdName} not found.");
		options.ClientSecret = builder.Configuration[SpotifyOptions.SpotifyClientSecretName] ?? throw new InvalidOperationException($"{SpotifyOptions.SpotifyClientSecretName} not found.");
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

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();

builder.Services.AddHostedService<SpotifyRefrehsBackgroundService>();


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

builder.Services.AddCookieConsent(o =>
{
    o.Revision = 1;
    o.PolicyUrl = "/cookie-policy";

    // Call optional
    o.UseDefaultConsentPrompt(prompt =>
    {
        prompt.Position = ConsentModalPosition.BottomRight;
        prompt.Layout = ConsentModalLayout.Cloud;
        prompt.SecondaryActionOpensSettings = true;
        prompt.AcceptAllButtonDisplaysFirst = true;
    });
	o.AutomaticallyShow = true;
	o.ConsentPromptVariant = new CustomSettingsPrompt();
});


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
