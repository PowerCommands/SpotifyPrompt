namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IUserManager
{
    /// <summary>
    /// Fetches the current user’s profile.
    /// Requires user-read-private (and user-read-email if you want email).
    /// </summary>
    UserProfile GetCurrentUser();
}