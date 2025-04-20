namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ISearchManager
{
    /// <summary>
    /// Search for tracks by query.
    /// </summary>
    List<TrackObject> SearchTracks(string query, int limit = 20);

    /// <summary>
    /// Search for artists by query.
    /// </summary>
    List<ArtistSimplified> SearchArtists(string query, int limit = 20);

    /// <summary>
    /// Search for albums by query.
    /// </summary>
    List<Album> SearchAlbums(string query, int limit = 20);

    /// <summary>
    /// Search for playlists by query.
    /// </summary>
    List<PlaylistInfo> SearchPlaylists(string query, int limit = 20);
}