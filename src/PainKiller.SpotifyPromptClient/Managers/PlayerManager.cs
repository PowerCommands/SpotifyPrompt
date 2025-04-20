using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
namespace PainKiller.SpotifyPromptClient.Managers;
public class PlayerManager : SpotifyClientBase, IPlayerManager
{
    private void SendCommand(HttpMethod method, string endpoint, object? content = null)
    {
        var accessToken = GetAccessToken();
        var deviceId = DeviceManager.Default.GetDeviceId();
        var url = $"{endpoint}?device_id={deviceId}";
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
    public void Play() => SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play");

    public void Pause() => SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/pause");

    public void Next() => SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next");

    public void Previous() => SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous");

    public (string? TrackName, string? Artists) GetCurrentlyPlaying()
    {
        var accessToken = GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/currently-playing");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        if (response.StatusCode == HttpStatusCode.NoContent)
            return (null, null);

        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        using var doc = JsonDocument.Parse(json);
        var itemElement = doc.RootElement.GetProperty("item");
        var name = itemElement.GetProperty("name").GetString();
        var artists = itemElement.GetProperty("artists")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToArray();
        return (name, string.Join(", ", artists));
    }
    public void SetShuffle(bool state, string? deviceId = null)
    {
        var accessToken = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/player/shuffle?state={state.ToString().ToLower()}";

        if (!string.IsNullOrEmpty(deviceId))
            url += $"&device_id={Uri.EscapeDataString(deviceId)}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
    public bool GetShuffleState(string? deviceId = null)
    {
        var accessToken = GetAccessToken();
        var url = "https://api.spotify.com/v1/me/player";

        if (!string.IsNullOrEmpty(deviceId)) url += $"?device_id={Uri.EscapeDataString(deviceId)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("shuffle_state").GetBoolean();
    }
}