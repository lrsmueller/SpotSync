using SpotifyAPI.Web;
using SpotSync.Data;

namespace SpotSync
{
    public class SpotifyService()
    {
        //change playlist
        //sync playlist
        //add playlist

        public async Task<List<SavedTrack>> GetLast100Tracks(SpotifyClient client)
        {
            var _1 = await client.Library.GetTracks(new LibraryTracksRequest()
            {
                Limit = 50,

            });
            var _2 = await client.Library.GetTracks(new LibraryTracksRequest()
            {
                Limit = 50,
                Offset = 50
            });
            var _ = new List<SavedTrack>();
            _.AddRange(_1.Items ?? new());
            _.AddRange(_2.Items ?? new());
            return _;
        }
        
    }
}
