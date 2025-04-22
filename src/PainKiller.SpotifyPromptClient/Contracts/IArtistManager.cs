namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IArtistManager
{
    /// <summary>
    /// Fetch a single artist’s full profile.
    /// Requires user-read-private scope if you ever want followers/email, otherwise public data only.
    /// </summary>
    Artist GetArtist(string artistId);

    /// <summary>
    /// Fetch multiple artists’ full profiles.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    List<Artist> GetArtists(IEnumerable<string> ids);
}