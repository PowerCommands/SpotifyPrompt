using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace PainKiller.SpotifyPromptClient.Managers;
public sealed class DeviceManager : SpotifyClientBase, IDeviceManager
{
    private DeviceManager() { }
    private static readonly Lazy<IDeviceManager> Instance = new(() => new DeviceManager());
    public static IDeviceManager Default => Instance.Value;
    
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
        foreach (var dev in devicesElem.EnumerateArray()) list.Add(new DeviceInfo { Id = dev.GetProperty("id").GetString()!, Name = dev.GetProperty("name").GetString()!, Type = dev.GetProperty("type").GetString()!, IsActive = dev.GetProperty("is_active").GetBoolean(), IsRestricted = dev.GetProperty("is_restricted").GetBoolean(), VolumePercent = dev.GetProperty("volume_percent").GetInt32() });
        return list;
    }
    public void SetActiveDevice(string deviceId, bool play = false)
    {
        var token = GetAccessToken();
        var uri = "https://api.spotify.com/v1/me/player";
        var body = new { device_ids = new[] { deviceId }, play };

        var req = new HttpRequestMessage(HttpMethod.Put, uri) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = _http.SendAsync(req).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();
    }
    public string GetDeviceId()
    {
        var accessToken = GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        var root = doc.RootElement;
        var devices = root.GetProperty("devices");

        
        var active = devices.EnumerateArray().FirstOrDefault(d => d.GetProperty("is_active").GetBoolean());
        if (active.ValueKind != JsonValueKind.Undefined) return active.GetProperty("id").GetString()!;
        
        var first = devices.EnumerateArray().FirstOrDefault();
        if (first.ValueKind != JsonValueKind.Undefined) return first.GetProperty("id").GetString()!;
        throw new InvalidOperationException("No available Spotify devices found.");
    }
    public void SetVolume(int volumePercent, string? deviceId = null)
    {
        if (volumePercent < 0 || volumePercent > 100) throw new ArgumentOutOfRangeException(nameof(volumePercent), "Volume must be between 0 and 100.");

        var accessToken = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/player/volume?volume_percent={volumePercent}";
        if (!string.IsNullOrEmpty(deviceId)) url += $"&device_id={Uri.EscapeDataString(deviceId)}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
    public int GetCurrentVolume(string? deviceId = null)
    {
        var devices = GetDevices();
        DeviceInfo device;
        if (!string.IsNullOrEmpty(deviceId)) device = devices.FirstOrDefault(d => d.Id == deviceId) ?? throw new InvalidOperationException($"No device found with ID {deviceId}.");
        else device = devices.FirstOrDefault(d => d.IsActive) ?? devices.FirstOrDefault() ?? throw new InvalidOperationException("No Spotify devices available.");
        return device.VolumePercent;
    }
}