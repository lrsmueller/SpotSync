using SpotifyAPI.Web;

namespace SpotSync.Interfaces
{
    public interface ISpotifyService
    {
        Task AddPlaylistAsync(PlaylistCreateRequest request, CancellationToken cancellationToken = default);
        Task<SpotifyClient> BuildClientAsync(CancellationToken cancellationToken = default);
        Task<List<SavedTrack>> GetLast100LikedSongsAsync(CancellationToken cancellationToken = default);
        Task<Paging<FullPlaylist>> GetPlaylistsAsync(CancellationToken cancellationToken = default);
        Task<PrivateUser> GetProfileAsync(CancellationToken cancellationToken = default);
        Task RefreshSelectedPlaylistAsync(CancellationToken cancellationToken = default);
    }
}