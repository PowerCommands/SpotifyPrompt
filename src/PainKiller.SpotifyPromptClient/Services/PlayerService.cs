using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.SpotifyPromptClient.Services;
public class PlayerService : SpotifyClientBase, IPlayerService
{
    private readonly ILogger<PlayerService> _logger = LoggerProvider.CreateLogger<PlayerService>();
    private void SendCommand(HttpMethod method, string endpoint, object? content = null)
    {
        var accessToken = GetAccessToken();
        var deviceId = DeviceService.Default.GetDeviceId();
        var url = $"{endpoint}?device_id={deviceId}";
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        var response = Http.SendAsync(request).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();
    }
    public void Play() => SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play");
    public void Play(string uri)
    {
        QueueService.Default.AddToQueue(uri);
        Next();
    }
    public void Play(IEnumerable<string> uris)
    {
        foreach (var u in uris) QueueService.Default.AddToQueue(u);
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

        var response = Http.SendAsync(request).GetAwaiter().GetResult();
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

        var response = Http.SendAsync(request).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("shuffle_state").GetBoolean();
    }
    public TrackObject? GetCurrentTrack()
    {
        var accessToken = GetAccessToken();
        var url = "https://api.spotify.com/v1/me/player/currently-playing";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = Http.SendAsync(request).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            // Nothing is playing, fall back to recently played
            var recentUrl = "https://api.spotify.com/v1/me/player/recently-played?limit=1";
            using var recentRequest = new HttpRequestMessage(HttpMethod.Get, recentUrl);
            recentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var recentResponse = Http.SendAsync(recentRequest).GetAwaiter().GetResult();
            _logger.LogInformation($"Recent Response: {recentResponse.StatusCode}");
            recentResponse.EnsureSuccessStatusCode();

            var recentJson = recentResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var recentDoc = JsonDocument.Parse(recentJson);

            var trackEl = recentDoc.RootElement.GetProperty("items")[0].GetProperty("track");
            return ParseTrack(trackEl);
        }

        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("item", out var item))
        {
            return ParseTrack(item);
        }

        return null;
    }

    private TrackObject ParseTrack(JsonElement trackEl)
    {
        var track = new TrackObject
        {
            Id = trackEl.GetProperty("id").GetString() ?? string.Empty,
            Name = trackEl.GetProperty("name").GetString() ?? string.Empty,
            Uri = trackEl.GetProperty("uri").GetString() ?? string.Empty,
            DurationMs = trackEl.GetProperty("duration_ms").GetInt32(),
            Album = new Album
            {
                Id = trackEl.GetProperty("album").GetProperty("id").GetString() ?? string.Empty,
                Name = trackEl.GetProperty("album").GetProperty("name").GetString() ?? string.Empty,
                Uri = trackEl.GetProperty("album").GetProperty("uri").GetString() ?? string.Empty,
                ReleaseDate = trackEl.GetProperty("album").GetProperty("release_date").GetString() ?? string.Empty
            }
        };

        foreach (var artistEl in trackEl.GetProperty("artists").EnumerateArray())
        {
            track.Artists.Add(new ArtistSimplified
            {
                Id = artistEl.GetProperty("id").GetString() ?? string.Empty,
                Name = artistEl.GetProperty("name").GetString() ?? string.Empty,
                Uri = artistEl.GetProperty("uri").GetString() ?? string.Empty
            });
        }

        return track;
    }
}