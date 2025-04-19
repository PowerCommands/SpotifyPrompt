using PainKiller.SpotifyPromptClient.DomainObjects;

namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IRefreshTokenManager
{
    /// <summary>
    /// Ensures the stored token is refreshed if expired or within the buffer period.
    /// Returns the current valid token and a status message.
    /// </summary>
    (TokenResponse Token, string Status) EnsureTokenValid();
}