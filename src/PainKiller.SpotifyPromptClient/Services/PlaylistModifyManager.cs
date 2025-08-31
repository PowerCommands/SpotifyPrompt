using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.SpotifyPromptClient.Services;

public class PlaylistModifyManager : SpotifyClientBase, IPlaylistModifyManager
{
    private readonly ILogger<PlaylistModifyManager> _logger = LoggerProvider.CreateLogger<PlaylistModifyManager>();
    private PlaylistModifyManager() { }
    private static readonly Lazy<IPlaylistModifyManager> Instance = new(() => new PlaylistModifyManager());
    public static IPlaylistModifyManager Default => Instance.Value;
    public string CreatePlaylist(string userId, string name, string description = "", bool isPublic = false)
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/users/{Uri.EscapeDataString(userId)}/playlists";
        var body = new
        {
            name,
            description,
            @public = isPublic
        };
        var payload = JsonSerializer.Serialize(body);
        
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }
    public void DeletePlaylist(string playlistId)
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/playlists/{Uri.EscapeDataString(playlistId)}/followers";

        using var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();
    }
    public void AddTracksToPlaylist(string playlistId, IEnumerable<string> trackUris, int position = -1)
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/playlists/{Uri.EscapeDataString(playlistId)}/tracks";
        var body = new Dictionary<string, object>
        {
            ["uris"] = trackUris.ToArray()
        };
        if (position >= 0) body["position"] = position;

        var payload = JsonSerializer.Serialize(body);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();
    }
    public void RemoveTracksFromPlaylist(string playlistId, IEnumerable<string> trackUris)
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/playlists/{Uri.EscapeDataString(playlistId)}/tracks";
        var trackObjects = trackUris.Select(uri => new { uri }).ToArray();
        var body = new { tracks = trackObjects };

        var payload = JsonSerializer.Serialize(body);
        using var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();
    }
}