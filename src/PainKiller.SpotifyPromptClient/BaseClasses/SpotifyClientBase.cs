using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.BaseClasses;
public abstract class SpotifyClientBase
{
    protected readonly HttpClient _http = new();
    protected string GetAccessToken()
    {
        var (token, status) = RefreshTokenManager.DefaultInstance().EnsureTokenValid();
        return token.AccessToken;
    }
}