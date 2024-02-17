using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using System.ComponentModel.DataAnnotations;

namespace SpotSync.Data
{
    public class SpotSyncContext(DbContextOptions<SpotSyncContext> options) : DbContext(options)
    {
        public DbSet<SpotifyAccount> Accounts { get; set; }

    }

    public class SpotifyAccount
    {
        [Key]
        public Guid Guid { get; set; }

        public string? SpotifyUserId { get; set; }

        //Sync 
        [Range(0, 23)]
        public int RefreshHour { get; set; }

        public DateTimeOffset LastRefresh { get; set; }

        public string? SelectedPlaylist { get; set; }




        // Access Token
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ExpiresIn { get; set; }

        public string TokenType { get; set; }

        public string Scope {get;set;}

        public AuthorizationCodeTokenResponse GetTokenResponse => new()
        {
            AccessToken = AccessToken,
            CreatedAt = CreatedAt.UtcDateTime,
            ExpiresIn = 3600,
            RefreshToken = RefreshToken,
            TokenType = TokenType,
            Scope = Scope,
        };
    }
}
