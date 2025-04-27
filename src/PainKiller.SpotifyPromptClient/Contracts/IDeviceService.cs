namespace PainKiller.SpotifyPromptClient.Contracts;
public interface IDeviceService
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
    /// <summary>
    /// Retrieves the currently active device ID.
    /// </summary>
    /// <returns></returns>
    string GetDeviceId();
    /// <summary>
    /// Sets the volume on the active device (or a specific device if you provide deviceId).
    /// </summary>
    /// <param name="volumePercent">Volume level between 0 and 100.</param>
    /// <param name="deviceId">Optional: ID of the device. If null, the active device is used.</param>
    void SetVolume(int volumePercent, string? deviceId = null);

    /// <summary>
    /// Retrieves the current volume level of the active device (or a specific device if you provide deviceId).
    /// </summary>
    /// <param name="deviceId">Optional: ID of the device. If null, the active device is used.</param>
    /// <returns>Volume level as an integer between 0 and 100.</returns>
    int GetCurrentVolume(string? deviceId = null);
}