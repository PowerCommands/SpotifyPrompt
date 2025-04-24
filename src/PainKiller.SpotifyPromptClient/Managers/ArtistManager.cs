using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
namespace PainKiller.SpotifyPromptClient.Managers;
public class ArtistManager : SpotifyClientBase, IArtistManager
{
    private readonly ILogger<ArtistManager> _logger = LoggerProvider.CreateLogger<ArtistManager>();

    private ArtistManager() { }
    private static readonly Lazy<IArtistManager> Instance = new(() => new ArtistManager());
    public static IArtistManager Default => Instance.Value;

    private const string BaseUrl = "https://api.spotify.com/v1/artists";
    public Artist GetArtist(string artistId)
    {
        var token = GetAccessToken();
        var url = $"{BaseUrl}/{Uri.EscapeDataString(artistId)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = _http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var artist = JsonSerializer.Deserialize<Artist>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return artist;
    }
    public List<Artist> GetArtists(IEnumerable<string> ids)
    {
        const int maxBatchSize = 50;
        var allIds = ids.ToList();
        var results = new List<Artist>();
        var token = GetAccessToken();

        for (var i = 0; i < allIds.Count; i += maxBatchSize)
        {
            var batch = allIds.Skip(i).Take(maxBatchSize);
            var url = $"https://api.spotify.com/v1/artists?ids={string.Join(',', batch)}";

            HttpResponseMessage resp;
            while (true)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                resp = _http.SendAsync(req).GetAwaiter().GetResult();
                if (resp.StatusCode == (HttpStatusCode)429)
                {
                    var wait = resp.Headers.RetryAfter?.Delta?.Seconds ?? 1;
                    Thread.Sleep(wait * 1000);
                    continue;
                }
                resp.EnsureSuccessStatusCode();
                break;
            }

            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.GetProperty("artists").EnumerateArray();
            foreach (var elem in arr)
            {
                if (elem.ValueKind != JsonValueKind.Object)
                {
                    _logger.LogWarning("Null value found, skip this object");
                    continue;
                }
                var artist = JsonSerializer.Deserialize<Artist>(
                    elem.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                )!;

                results.Add(artist);
            }
        }
        return results;
    }
    public List<TrackObject> GetTopTracks(string artistId, string market = "US")
    {
        var token = GetAccessToken();
        var url = $"{BaseUrl}/{Uri.EscapeDataString(artistId)}/top-tracks?market={Uri.EscapeDataString(market)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = _http.SendAsync(req).GetAwaiter().GetResult();
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        return doc.RootElement
            .GetProperty("tracks")
            .EnumerateArray()
            .Select(ParseTrack)
            .ToList();
    }
    public ArtistSimplified GetArtistByName(string artistName)
    {
        var searchResult = SearchManager.Default.SearchArtists(artistName);
        foreach (var artistSimplified in searchResult.Where(artistSimplified => artistSimplified.Name.Trim() == artistName)) return artistSimplified;
        return new ArtistSimplified();
    }
    private TrackObject ParseTrack(JsonElement item)
    {
        return new TrackObject
        {
            Id = item.GetProperty("id").GetString() ?? string.Empty,
            Name = item.GetProperty("name").GetString() ?? string.Empty,
            DurationMs = item.GetProperty("duration_ms").GetInt32(),
            Uri = item.GetProperty("uri").GetString() ?? string.Empty,
            Artists = item.GetProperty("artists").EnumerateArray()
                .Select(a => new ArtistSimplified
                {
                    Id = a.GetProperty("id").GetString() ?? string.Empty,
                    Name = a.GetProperty("name").GetString() ?? string.Empty,
                    Uri = a.GetProperty("uri").GetString() ?? string.Empty
                })
                .ToList(),
            Album = new Album
            {
                Id = item.GetProperty("album").GetProperty("id").GetString() ?? string.Empty,
                Name = item.GetProperty("album").GetProperty("name").GetString() ?? string.Empty,
                ReleaseDate = item.GetProperty("album").GetProperty("release_date").GetString() ?? string.Empty,
                TotalTracks = item.GetProperty("album").GetProperty("total_tracks").GetInt32(),
                Uri = item.GetProperty("album").GetProperty("uri").GetString() ?? string.Empty,
                Artists = item.GetProperty("album").GetProperty("artists").EnumerateArray()
                    .Select(a => new ArtistSimplified
                    {
                        Id = a.GetProperty("id").GetString() ?? string.Empty,
                        Name = a.GetProperty("name").GetString() ?? string.Empty,
                        Uri = a.GetProperty("uri").GetString() ?? string.Empty
                    })
                    .ToList()
            }
        };
    }
}