using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.BaseClasses;
public abstract class SpotifyClientBase
{
    protected readonly HttpClient Http = new();
    protected string GetAccessToken()
    {
        var (token, status) = RefreshTokenService.DefaultInstance().EnsureTokenValid();
        return token.AccessToken;
    }
}