namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ISelectedService
{
    /// <summary>
    /// Store selected tracks, and persist related tracks, albums, and artists.
    /// </summary>
    void Store(IEnumerable<TrackObject> tracks);

    /// <summary>
    /// Replace current selected tracks.
    /// </summary>
    void UpdateSelected(List<TrackObject> tracks);

    /// <summary>
    /// Append tracks to current selection.
    /// </summary>
    void AppendToSelected(List<TrackObject> tracks);

    /// <summary>
    /// Get currently selected tracks.
    /// </summary>
    List<TrackObject> GetSelectedTracks();

    /// <summary>
    /// Replace current selected albums.
    /// </summary>
    void UpdateSelected(List<Album> albums);

    /// <summary>
    /// Append albums to current selection.
    /// </summary>
    void AppendToSelected(List<Album> albums);

    /// <summary>
    /// Get currently selected albums.
    /// </summary>
    List<Album> GetSelectedAlbums();

    /// <summary>
    /// Replace current selected artists.
    /// </summary>
    void UpdateSelected(List<ArtistSimplified> artists);

    /// <summary>
    /// Append artists to current selection.
    /// </summary>
    void AppendToSelected(List<ArtistSimplified> artists);

    /// <summary>
    /// Get currently selected artists.
    /// </summary>
    List<ArtistSimplified> GetSelectedArtists();
    void UpdateLatestPlaying(TrackObject track, int latestTracksCount);
}
