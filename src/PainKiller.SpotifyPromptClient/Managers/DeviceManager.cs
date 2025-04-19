using PainKiller.SpotifyPromptClient.DomainObjects;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using PainKiller.SpotifyPromptClient.BaseClasses;
using PainKiller.SpotifyPromptClient.Contracts;

namespace PainKiller.SpotifyPromptClient.Managers;

public sealed class DeviceManager : SpotifyClientBase, IDeviceManager
{
    private DeviceManager() { }
    private static readonly Lazy<IDeviceManager> Instance = new(() => new DeviceManager());
    public static IDeviceManager Default => Instance.Value;

    /// <summary>
    /// Retrieves all available devices for the user.
    /// </summary>
    public List<DeviceInfo> GetDevices()
    {
        var token = GetAccessToken();
        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = _http.SendAsync(req).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();

        var json = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        var devicesElem = doc.RootElement.GetProperty("devices");

        var list = new List<DeviceInfo>();
        foreach (var dev in devicesElem.EnumerateArray())
        {
            list.Add(new DeviceInfo
            {
                Id = dev.GetProperty("id").GetString()!,
                Name = dev.GetProperty("name").GetString()!,
                Type = dev.GetProperty("type").GetString()!,
                IsActive = dev.GetProperty("is_active").GetBoolean(),
                IsRestricted = dev.GetProperty("is_restricted").GetBoolean(),
                VolumePercent = dev.GetProperty("volume_percent").GetInt32()
            });
        }

        return list;
    }
    /// <summary>
    /// Sets the specified device as active (transfers playback).
    /// Optionally starts playback immediately.
    /// </summary>
    public void SetActiveDevice(string deviceId, bool play = false)
    {
        var token = GetAccessToken();
        var uri = "https://api.spotify.com/v1/me/player";
        var body = new { device_ids = new[] { deviceId }, play };

        var req = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = _http.SendAsync(req).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();
    }
}