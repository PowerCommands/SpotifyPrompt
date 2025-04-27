namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IRefreshTokenService
{
    /// <summary>
    /// Ensures the stored token is refreshed if expired or within the buffer period.
    /// Returns the current valid token and a status message.
    /// </summary>
    (TokenResponse Token, string Status) EnsureTokenValid();
}