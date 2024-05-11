using SpotifyAPI.Web;
using System.ComponentModel.DataAnnotations;

namespace SpotSync.Models;

public class AddPlaylistModel
{
    [Required]
    public string Name { get; set; }

    public bool Public { get; set; } = false;

    public bool Collaborative { get; set; } = false;

    public string? Description { get; set; } = string.Empty;


    public PlaylistCreateRequest ToRequest()
    {
        return new(Name)
        {
            Public = Public,
            Collaborative = Collaborative,
            Description = Description
        };
    }
}
