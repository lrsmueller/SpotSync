namespace SpotSync;

public class SpotifySettings
{

    public static  readonly string SpotifyClientIdName = "SPOTIFY_CLIENT_ID";
        public static readonly string SpotifyClientSecretName = "SPOTIFY_CLIENT_SECRET";

    public string SpotifyClientId { get; set; }

    public string SpotifyClientSecret { get; set; }
}
