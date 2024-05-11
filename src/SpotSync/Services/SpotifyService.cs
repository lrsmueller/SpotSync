using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using SpotSync.Common;
using SpotSync.Data;
using SpotSync.Interfaces;

namespace SpotSync.Services
{
    public class SpotifyService : ISpotifyService
    {
        public readonly int ExpiresSeconds = -3600;

        private readonly IUserService _userService;
        private readonly IOptions<SpotifyOptions> _spotifySettings;

        private SpotifyClient SpotifyClient;

        public List<string> scopes = [Scopes.UserReadEmail,
                    Scopes.UserReadPrivate,
                    Scopes.PlaylistReadPrivate,
                    Scopes.PlaylistReadCollaborative,
                    Scopes.PlaylistModifyPrivate,
                    Scopes.PlaylistModifyPublic,
                    Scopes.UserLibraryRead];

        public SpotifyService(IUserService userService, IOptions<SpotifyOptions> options)
        {
            _userService = userService;
            _spotifySettings = options;
        }

        public async Task<SpotifyClient> BuildClientAsync(CancellationToken cancellationToken = default)
        {
            if (SpotifyClient != null) { return SpotifyClient; }



            var tokens = await _userService.GetUserTokensAsync(cancellationToken);
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithHTTPLogger(new SimpleConsoleHTTPLogger())
                .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifySettings.Value.SpotifyClientId, _spotifySettings.Value.SpotifyClientSecret, tokens.CreateAuthResponse()));
            SpotifyClient = new SpotifyClient(config);

            return SpotifyClient;
        }


        public async Task RefreshSelectedPlaylistAsync(CancellationToken cancellationToken = default)
        {
            if (SpotifyClient == null)
            {
                SpotifyClient = await BuildClientAsync(cancellationToken);

            }
            var user = await _userService.GetUserAsync(cancellationToken);
            //var tracks = await GetLast100LikedSongsAsync(cancellationToken);
            //var request = new PlaylistReplaceItemsRequest(tracks
            //    .Select(e => e.Track.Uri)
            //    .ToList());

            //await SpotifyClient.Playlists.ReplaceItems(user.SelectedPlaylist, request, cancellationToken);

            await RefreshSelectedPlaylistAsync(SpotifyClient,user.SelectedPlaylist, cancellationToken);
        }

        public async Task<List<SavedTrack>> GetLast100LikedSongsAsync(CancellationToken cancellationToken = default)
        {
            return await GetLast100LikedSongsAsync(SpotifyClient, cancellationToken);
        }

        public async Task<Paging<FullPlaylist>> GetPlaylistsAsync(CancellationToken cancellationToken = default)
        {
            return await GetPlaylists(SpotifyClient, cancellationToken);
        }

        public async Task AddPlaylistAsync(PlaylistCreateRequest request, CancellationToken cancellationToken = default)
        {
            await AddPlaylistAsync(SpotifyClient, request, cancellationToken);
        }

        public async Task<PrivateUser> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            return await GetProfileAsync(SpotifyClient, cancellationToken);
        }


        public static async Task RefreshSelectedPlaylistAsync(SpotifyClient client, string playlist, CancellationToken cancellationToken = default)
        {
            
            var tracks = await GetLast100LikedSongsAsync(client,cancellationToken);
            var request = new PlaylistReplaceItemsRequest(tracks
                .Select(e => e.Track.Uri)
                .ToList());

            await client.Playlists.ReplaceItems(playlist, request, cancellationToken);
        }

        public static async Task<List<SavedTrack>> GetLast100LikedSongsAsync(SpotifyClient client, CancellationToken cancellationToken = default)
        {
            var _1 = await client.Library.GetTracks(new LibraryTracksRequest()
            {
                Limit = 50,

            }, cancellationToken);
            var _2 = await client.Library.GetTracks(new LibraryTracksRequest()
            {
                Limit = 50,
                Offset = 50
            }, cancellationToken);
            return [.. _1.Items, .. _2.Items];
        }

        public static async Task<FullPlaylist> AddPlaylistAsync(SpotifyClient client, PlaylistCreateRequest request, CancellationToken cancellationToken)
        {
            var profile = await client.UserProfile.Current(cancellationToken);

            return await client.Playlists.Create(profile.Id, request, cancellationToken);
        }

        public static async Task<Paging<FullPlaylist>> GetPlaylists(SpotifyClient client, CancellationToken cancellationToken = default)
        {
            return await client.Playlists.CurrentUsers(cancellationToken);
        }

        public static async Task<PrivateUser> GetProfileAsync(SpotifyClient client, CancellationToken cancellationToken = default)
        {
            return await client.UserProfile.Current(cancellationToken);
        }

    }
}
