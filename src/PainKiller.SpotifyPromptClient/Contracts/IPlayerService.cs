namespace PainKiller.SpotifyPromptClient.Contracts;

/// <summary>
/// Wraps playback control calls to Spotify Web API, using the currently stored access token.
/// Attempts to select an active device; if none is active, picks the first available device.
/// Assumes token is kept fresh by InfoPanel refresh thread.
/// </summary>
public interface IPlayerService
{
    void Play();
    void Play(string uri);
    void Play(IEnumerable<string> uris);
    void Pause();
    void Next();
    void Previous();
    /// <summary>
    /// Enable or disable shuffle (randomize) on the user’s player.
    /// </summary>
    /// <param name="state">True to turn shuffle on, false to turn it off.</param>
    /// <param name="deviceId">Optional device ID. If null, Spotify will use the active device.</param>
    void SetShuffle(bool state, string? deviceId = null);

    /// <summary>
    /// Get the current shuffle state of the user’s player.
    /// </summary>
    /// <param name="deviceId">Optional device ID. If provided, returns the shuffle state for that device.</param>
    /// <returns>True if shuffle is on, false if off.</returns>
    bool GetShuffleState(string? deviceId = null);
    TrackObject? GetCurrentTrack();
}