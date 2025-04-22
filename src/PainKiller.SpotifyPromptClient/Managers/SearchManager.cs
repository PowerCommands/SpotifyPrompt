using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
namespace PainKiller.SpotifyPromptClient.Managers;
public class SearchManager : SpotifyClientBase, ISearchManager
{
    private readonly ILogger<SearchManager> _logger = LoggerProvider.CreateLogger<SearchManager>();

    private SearchManager() { }
    private static readonly Lazy<ISearchManager> Instance = new(() => new SearchManager());
    public static ISearchManager Default => Instance.Value;

    private const string BaseUrl = "https://api.spotify.com/v1/search";
    public List<TrackObject> SearchTracks(string query, int limit = 20)
    {
        var json = SendSearchRequest(query, "track", limit);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray().Select(ParseTrack).ToList();
    }
    public List<ArtistSimplified> SearchArtists(string query, int limit = 20)
    {
        var json = SendSearchRequest(query, "artist", limit);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("artists").GetProperty("items").EnumerateArray().Select(item => new ArtistSimplified { Id   = item.GetProperty("id").GetString()   ?? "", Name = item.GetProperty("name").GetString() ?? "", Uri  = item.GetProperty("uri").GetString()  ?? "" }).ToList();
    }
    public List<Album> SearchAlbums(string query, int limit = 20)
    {
        var json = SendSearchRequest(query, "album", limit);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("albums").GetProperty("items").EnumerateArray().Select(item => new Album { Id = item.GetProperty("id").GetString() ?? "", Name = item.GetProperty("name").GetString() ?? "", ReleaseDate = item.GetProperty("release_date").GetString() ?? "", TotalTracks = item.GetProperty("total_tracks").GetInt32(), Uri = item.GetProperty("uri").GetString() ?? "", Artists = item.GetProperty("artists").EnumerateArray().Select(a => new ArtistSimplified { Id = a.GetProperty("id").GetString() ?? "", Name = a.GetProperty("name").GetString() ?? "", Uri  = a.GetProperty("uri").GetString() ?? "" }).ToList() }).ToList();
    }
    public List<PlaylistInfo> SearchPlaylists(string query, int limit = 20)
    {
        var json = SendSearchRequest(query, "playlist", limit);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("playlists").GetProperty("items").EnumerateArray().Select(item => new PlaylistInfo { Id = item.GetProperty("id").GetString()   ?? "", Name = item.GetProperty("name").GetString() ?? "", Owner = item.GetProperty("owner").GetProperty("display_name").GetString() ?? "", TrackCount = item.GetProperty("tracks").GetProperty("total").GetInt32() }).ToList();
    }
    private string SendSearchRequest(string query, string type, int limit)
    {
        var token = GetAccessToken();
        var url   = $"{BaseUrl}?q={Uri.EscapeDataString(query)}&type={type}&limit={limit}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = _http.SendAsync(req).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();
        return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
    private TrackObject ParseTrack(JsonElement item)
    {
        var trackElem = item;
        return new TrackObject { Id = trackElem.GetProperty("id").GetString() ?? "", Name = trackElem.GetProperty("name").GetString() ?? "", DurationMs = trackElem.GetProperty("duration_ms").GetInt32(), Uri = trackElem.GetProperty("uri").GetString() ?? "", Artists    = trackElem.GetProperty("artists").EnumerateArray().Select(a => new ArtistSimplified { Id = a.GetProperty("id").GetString()   ?? "", Name = a.GetProperty("name").GetString() ?? "", Uri  = a.GetProperty("uri").GetString()  ?? "" }).ToList(), Album = new Album { Id = trackElem.GetProperty("album").GetProperty("id").GetString()   ?? "", Name = trackElem.GetProperty("album").GetProperty("name").GetString() ?? "", ReleaseDate = trackElem.GetProperty("album").GetProperty("release_date").GetString() ?? "", TotalTracks = trackElem.GetProperty("album").GetProperty("total_tracks").GetInt32(), Uri = trackElem.GetProperty("album").GetProperty("uri").GetString() ?? "", Artists = trackElem.GetProperty("album").GetProperty("artists").EnumerateArray().Select(a2 => new ArtistSimplified { Id   = a2.GetProperty("id").GetString()   ?? "", Name = a2.GetProperty("name").GetString() ?? "", Uri  = a2.GetProperty("uri").GetString()  ?? "" }).ToList() } };
    }
}