using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using SpotSync.Data;

namespace SpotSync
{
    public class SyncService
    {
        private readonly SpotSyncContext _context;
        private readonly SpotifyService _spotifyService;

        public SyncService(SpotSyncContext context, SpotifyService spotifyService)
        {
            _context = context;
            _spotifyService = spotifyService;
        }

        public string clientId => Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? string.Empty;

        public string clientSecret => Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? string.Empty;

        public async Task SyncTopSongs(Guid guid)
        {


            var user = _context.Accounts
                .Where(e=>e.Guid == guid)
                .FirstOrDefault();
            if (user is not null && !string.IsNullOrWhiteSpace(user.SelectedPlaylist)) 
            {
                var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(new AuthorizationCodeAuthenticator(clientId, clientSecret, user.GetTokenResponse));

                var SpotifyClient = new SpotifyClient(config);


                var tracks = await _spotifyService.GetLast100Tracks(SpotifyClient);

                var trackUris = new PlaylistReplaceItemsRequest(tracks
                    .Select(e => e.Track.Uri)
                    .ToList());
                try
                {

                await SpotifyClient.Playlists.ReplaceItems(user.SelectedPlaylist, trackUris);
                } catch (Exception ex)
                {
                    var _ = ex;
                }

                user.LastRefresh = DateTimeOffset.UtcNow;
            }

        }
    }
}
