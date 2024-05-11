using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpotSync.Common;
using SpotSync.Data;
using SpotSync.Interfaces;

namespace SpotSync.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly AuthenticationStateProvider _authentication;

    public UserService(IDbContextFactory<ApplicationDbContext> dbContextFactory, AuthenticationStateProvider authentication)
    {
        _dbContextFactory = dbContextFactory;
        _authentication = authentication;
    }

    private async Task<string> GetUserId()
    {
        //TODO more null checking
        var state = await _authentication.GetAuthenticationStateAsync();
        if (!state.User.Identity.IsAuthenticated) throw new Exception("User not Found");

        var id = state.User.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        return id;
    }

    public async Task<ApplicationUser> GetUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = await GetUserId();
        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Users.FindAsync(userId, cancellationToken);
    }

    public async Task<Dictionary<string, IdentityUserToken<string>>> GetUserTokensAsync(CancellationToken cancellationToken = default)
    {
        var userId = await GetUserId();

        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var tokens = context.UserTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.LoginProvider == "Spotify")
            .ToDictionary(x => x.Name);

        if (!tokens.ContainsKey(SpotifyToken.Access) || !tokens.ContainsKey(SpotifyToken.Refresh)
            || !tokens.ContainsKey(SpotifyToken.Type) || !tokens.ContainsKey(SpotifyToken.ExpiresAt))
        {
            throw new Exception("Tokens not set");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[SpotifyToken.Access].Value, SpotifyToken.Access);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[SpotifyToken.Type].Value, SpotifyToken.Type);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[SpotifyToken.Refresh].Value, SpotifyToken.Refresh);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[SpotifyToken.ExpiresAt].Value, SpotifyToken.ExpiresAt);

        return tokens;
    }

    public async Task UpdatePlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(cancellationToken);

        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        user.SelectedPlaylist = playlistId;
        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRefreshTimeAsync(int selectedHour, CancellationToken cancellationToken = default)
    {
        if (selectedHour >= 24 || selectedHour < 0) { selectedHour = 0; }
        var user = await GetUserAsync(cancellationToken);

        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        user.RefreshHour = selectedHour;
        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);

    }

    public async Task UserPlaylistRefreshedAsync(DateTimeOffset lastRefresh, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(cancellationToken);
        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        user.LastRefresh = lastRefresh;
        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
