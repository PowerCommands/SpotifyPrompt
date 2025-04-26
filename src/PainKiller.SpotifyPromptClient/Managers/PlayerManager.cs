using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
namespace PainKiller.SpotifyPromptClient.Managers;
public class PlayerManager : SpotifyClientBase, IPlayerManager
{
    private readonly ILogger<PlayerManager> _logger = LoggerProvider.CreateLogger<PlayerManager>();
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
        _logger.LogInformation($"Response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();
    }
    public void Play() => SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play");
    public void Play(string uri)
    {
        QueueManager.Default.AddToQueue(uri);
        Next();
    }
    public void Play(IEnumerable<string> uris)
    {
        foreach (var u in uris) QueueManager.Default.AddToQueue(u);
        Next();
    }
    public void Pause() => SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/pause");

    public void Next() => SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next");

    public void Previous() => SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous");
    public void SetShuffle(bool state, string? deviceId = null)
    {
        var accessToken = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/player/shuffle?state={state.ToString().ToLower()}";

        if (!string.IsNullOrEmpty(deviceId))
            url += $"&device_id={Uri.EscapeDataString(deviceId)}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {response.StatusCode}");
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
        _logger.LogInformation($"Response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("shuffle_state").GetBoolean();
    }
}