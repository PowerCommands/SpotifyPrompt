using PainKiller.SpotifyPromptClient.DomainObjects;

namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IAuthorizationCodeFlowManager
{
    Task<string> AuthenticateAsync();
    Task<TokenResponse> ExchangeCodeForTokenAsync(string code);
    Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken);
}