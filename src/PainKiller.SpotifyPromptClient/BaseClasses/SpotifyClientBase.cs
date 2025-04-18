using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.BaseClasses;
public abstract class SpotifyClientBase(RefreshTokenManager refreshManager)
{
    protected readonly HttpClient _http = new();
    protected string GetAccessToken()
    {
        var (token, status) = refreshManager.EnsureTokenValid();
        return token.AccessToken;
    }
}