using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using SpotSync.Data;

namespace SpotSync
{
    public class AccountService
    {
        private readonly SpotSyncContext _context;

        public AccountService(SpotSyncContext context)
        {
            _context = context;
        }
        public async Task<SpotifyAccount> GetOrCreateAccount(PrivateUser privateUser, AuthorizationCodeTokenResponse response)
        {
            var _ = _context.Accounts.Where(e => privateUser.Id == e.SpotifyUserId).FirstOrDefault();
            if(_ == null)
            {
                _ = new SpotifyAccount()
                {
                    SpotifyUserId = privateUser.Id,
                    Guid = Guid.NewGuid(),

                    AccessToken = response.AccessToken,
                    CreatedAt = response.CreatedAt,
                    ExpiresIn = response.CreatedAt.AddSeconds(response.ExpiresIn),
                    RefreshToken = response.RefreshToken,
                    TokenType = response.TokenType,
                    Scope = response.Scope

                };
                _context.Accounts.Add(_);
                
            } else
            {
                _.AccessToken = response.AccessToken;
                _.CreatedAt = response.CreatedAt;
                _.ExpiresIn = response.CreatedAt.AddSeconds(response.ExpiresIn);
                _.RefreshToken = response.RefreshToken;
                _.TokenType = response.TokenType;
                _.Scope = response.Scope;
            }

            await _context.SaveChangesAsync();
            return _;
        }

        public async Task<AuthorizationCodeTokenResponse> GetResponseForUser(Guid guid)
        {
            var _ = await _context.Accounts
                .AsNoTracking()
                .Where(e=>e.Guid == guid)
                .FirstOrDefaultAsync();

            return _.GetTokenResponse;
        }

        public async Task UpdatePlaylist(Guid guid,string playlist)
        {
            var _ = await _context.Accounts
                .Where(e => e.Guid == guid)
                .FirstOrDefaultAsync();

            _.SelectedPlaylist = playlist;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSyncHour(Guid guid, int Hour)
        {
            var _ = await _context.Accounts
                .Where(e => e.Guid == guid)
                .FirstOrDefaultAsync();

            _.RefreshHour = Hour;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUser(Guid guid)
        {
            await _context.Accounts
                .Where(e => e.Guid == guid)
                .ExecuteDeleteAsync();
        }
    }
}
