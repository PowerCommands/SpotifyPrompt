namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IPlaylistService
{
    /// <summary>
    /// Retrieves all playlists for the current user, paging through results 50 at a time.
    /// </summary>
    List<PlaylistInfo> GetAllPlaylists();

    /// <summary>
    /// Starts playback of the specified playlist on the given device (or default device if none specified).
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist to play.</param>
    /// <param name="deviceId">Optional device ID to start playback on.</param>
    void PlayPlaylist(string playlistId, string? deviceId = null);

    /// <summary>
    /// Get all tracks for a given playlist.
    /// </summary>
    /// <param name="playlistId">The spotify ID for the given playlist.</param>
    /// <returns>All track objects that the playlist contains.</returns>
    List<TrackObject> GetAllTracksForPlaylist(string playlistId);
}