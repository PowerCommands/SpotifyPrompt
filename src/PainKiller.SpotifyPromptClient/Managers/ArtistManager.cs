using System.Net.Http.Headers;
using System.Text.Json;
namespace PainKiller.SpotifyPromptClient.Managers;
public class ArtistManager : SpotifyClientBase, IArtistManager
{
    private ArtistManager() { }
    private static readonly Lazy<IArtistManager> Instance = new(() => new ArtistManager());
    public static IArtistManager Default => Instance.Value;

    private const string BaseUrl = "https://api.spotify.com/v1/artists";
    public Artist GetArtist(string artistId)
    {
        var token = GetAccessToken();
        var url   = $"{BaseUrl}/{Uri.EscapeDataString(artistId)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = _http.SendAsync(req).GetAwaiter().GetResult();
        resp.EnsureSuccessStatusCode();

        var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var artist = JsonSerializer.Deserialize<Artist>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return artist;
    }
}