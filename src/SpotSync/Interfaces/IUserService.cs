using Microsoft.AspNetCore.Identity;
using SpotSync.Data;

namespace SpotSync.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, IdentityUserToken<string>>> GetUserTokensAsync(CancellationToken cancellationToken = default);
        Task UpdatePlaylistAsync(string playlistId, CancellationToken cancellationToken = default);
        Task UpdateRefreshTimeAsync(int selectedHour, CancellationToken cancellationToken = default);
        Task UserPlaylistRefreshedAsync(DateTimeOffset lastRefresh, CancellationToken cancellationToken);
    }
}