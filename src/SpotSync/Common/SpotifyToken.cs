using Microsoft.AspNetCore.Identity;
using SpotifyAPI.Web;

namespace SpotSync.Common;

public static class SpotifyToken
{
    public static readonly string Access = "access_token";
    public static readonly string Refresh = "refresh_token";
    public static readonly string Type = "token_type";
    public static readonly string ExpiresAt = "expires_at";
    public static readonly int ExpiresSeconds = -3600;
    public static readonly List<string> SpotifyScopes = [Scopes.UserReadEmail, Scopes.UserReadPrivate, Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic, Scopes.UserLibraryRead];

    public static AuthorizationCodeTokenResponse CreateAuthResponse(this List<IdentityUserToken<string>> tokens)
    {

        return CreateAuthResponse(tokens.ToDictionary(x => x.Name));
    }

    public static AuthorizationCodeTokenResponse CreateAuthResponse(this Dictionary<string,IdentityUserToken<string>> tokens)
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[Access].Value, SpotifyToken.Access);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[Type].Value, SpotifyToken.Type);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[Refresh].Value, SpotifyToken.Refresh);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokens[ExpiresAt].Value, SpotifyToken.ExpiresAt);

        var response = new AuthorizationCodeTokenResponse()
        {
            AccessToken = tokens[Access].Value,
            TokenType = tokens[Type].Value,
            RefreshToken = tokens[Refresh].Value,
            ExpiresIn = 3600,
            Scope = string.Join(",", SpotifyScopes)
        };
        response.CreatedAt = DateTimeOffset.Parse(tokens[ExpiresAt].Value).DateTime.AddSeconds(ExpiresSeconds);

        return response;
    }
}
