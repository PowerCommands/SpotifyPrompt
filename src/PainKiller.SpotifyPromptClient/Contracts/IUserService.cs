namespace PainKiller.SpotifyPromptClient.Contracts;
public interface IUserService
{
    /// <summary>
    /// Fetches the current user’s profile.
    /// Requires user-read-private (and user-read-email if you want email).
    /// </summary>
    UserProfile GetCurrentUser();
    /// <summary>
    /// Fetches the current user’s top tracks.
    /// </summary>
    List<TrackObject> GetTopTracks(int limit = 20, string timeRange = "medium_term");

    /// <summary>
    /// Fetches the current user’s top artists.
    /// </summary>
    List<ArtistSimplified> GetTopArtists(int limit = 20, string timeRange = "medium_term");
}