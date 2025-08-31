using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.SpotifyPromptClient.Services;

public class UserService : SpotifyClientBase, IUserService
{
    private readonly ILogger<UserService> _logger = LoggerProvider.CreateLogger<UserService>();
    private UserService() { }
    private static readonly Lazy<IUserService> Instance = new(() => new UserService());
    public static IUserService Default => Instance.Value;
    public UserProfile GetCurrentUser()
    {
        var token = GetAccessToken();
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();
        var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    public List<TrackObject> GetTopTracks(int limit = 20, string timeRange = "medium_term")
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/top/tracks?limit={limit}&time_range={timeRange}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        return doc.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(item => new TrackObject
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? "",
                DurationMs = item.GetProperty("duration_ms").GetInt32(),
                Uri = item.GetProperty("uri").GetString() ?? "",
                Artists = item.GetProperty("artists")
                                 .EnumerateArray()
                                 .Select(a => new ArtistSimplified
                                 {
                                     Id = a.GetProperty("id").GetString() ?? "",
                                     Name = a.GetProperty("name").GetString() ?? "",
                                     Uri = a.GetProperty("uri").GetString() ?? ""
                                 }).ToList(),
                Album = new Album
                {
                    Id = item.GetProperty("album").GetProperty("id").GetString() ?? "",
                    Name = item.GetProperty("album").GetProperty("name").GetString() ?? "",
                    ReleaseDate = item.GetProperty("album").GetProperty("release_date").GetString() ?? "",
                    TotalTracks = item.GetProperty("album").GetProperty("total_tracks").GetInt32(),
                    Uri = item.GetProperty("album").GetProperty("uri").GetString() ?? "",
                    Artists = item.GetProperty("album").GetProperty("artists")
                                       .EnumerateArray()
                                       .Select(a2 => new ArtistSimplified
                                       {
                                           Id = a2.GetProperty("id").GetString() ?? "",
                                           Name = a2.GetProperty("name").GetString() ?? "",
                                           Uri = a2.GetProperty("uri").GetString() ?? ""
                                       }).ToList()
                }
            }).ToList();
    }
    public List<ArtistSimplified> GetTopArtists(int limit = 20, string timeRange = "medium_term")
    {
        var token = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/top/artists?limit={limit}&time_range={timeRange}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        return doc.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(item => new ArtistSimplified
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? "",
                Uri = item.GetProperty("uri").GetString() ?? ""
            }).ToList();
    }
}