﻿namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IPlayerManager
{
    void Play();
    void Pause();
    void Next();
    void Previous();
    (string? TrackName, string? Artists) GetCurrentlyPlaying();

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
}