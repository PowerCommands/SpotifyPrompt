namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IAuthorizationCodeFlowService
{
    Task<string> AuthenticateAsync();
    Task<TokenResponse> ExchangeCodeForTokenAsync(string code);
    Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken);
}