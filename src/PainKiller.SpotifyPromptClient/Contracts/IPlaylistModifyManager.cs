namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IPlaylistModifyManager
{
    /// <summary>
    /// Creates a new playlist for the given user.
    /// Returns the new playlist’s ID.
    /// </summary>
    string CreatePlaylist(string userId, string name, string description = "", bool isPublic = false);

    /// <summary>
    /// “Deletes” a playlist by unfollowing it.
    /// Spotify does not support hard‑deletes, so this removes the current user as a follower.
    /// </summary>
    void DeletePlaylist(string playlistId);

    /// <summary>
    /// Adds the specified track URIs to the playlist.
    /// Optionally at the given zero‑based position (appends if position &lt; 0).
    /// </summary>
    void AddTracksToPlaylist(string playlistId, IEnumerable<string> trackUris, int position = -1);

    /// <summary>
    /// Removes the specified track URIs from the playlist.
    /// </summary>
    void RemoveTracksFromPlaylist(string playlistId, IEnumerable<string> trackUris);
}