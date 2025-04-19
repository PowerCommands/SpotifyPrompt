using PainKiller.SpotifyPromptClient.DomainObjects;

namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IDeviceManager
{
    /// <summary>
    /// Retrieves all available devices for the user.
    /// </summary>
    List<DeviceInfo> GetDevices();

    /// <summary>
    /// Sets the specified device as active (transfers playback).
    /// Optionally starts playback immediately.
    /// </summary>
    void SetActiveDevice(string deviceId, bool play = false);
}