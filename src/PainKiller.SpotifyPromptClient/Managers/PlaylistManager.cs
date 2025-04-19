using PainKiller.SpotifyPromptClient.DomainObjects;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PainKiller.SpotifyPromptClient.BaseClasses;
using PainKiller.SpotifyPromptClient.Contracts;

namespace PainKiller.SpotifyPromptClient.Managers;

public class PlaylistManager : SpotifyClientBase, IPlaylistManager
{
    private const string BaseUrl = "https://api.spotify.com/v1/me/playlists";

    private static readonly Lazy<IPlaylistManager> Instance = new(() => new PlaylistManager());
    public static IPlaylistManager Default => Instance.Value;

    /// <summary>
    /// Retrieves all playlists for the current user, paging through results 50 at a time.
    /// </summary>
    public List<PlaylistInfo> GetAllPlaylists()
    {
        var playlists = new List<PlaylistInfo>();
        var accessToken = GetAccessToken();
        var nextUrl = BaseUrl + "?limit=50";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, nextUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = _http.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Parse items
            if (root.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var info = new PlaylistInfo
                    {
                        Id = item.GetProperty("id").GetString()!,
                        Name = item.GetProperty("name").GetString()!,
                        Owner = item.GetProperty("owner").GetProperty("display_name").GetString()!,
                        TrackCount = item.GetProperty("tracks").GetProperty("total").GetInt32()
                    };
                    playlists.Add(info);
                }
            }

            // Determine next page URL
            nextUrl = root.GetProperty("next").GetString();
        }

        return playlists;
    }

    /// <summary>
    /// Starts playback of the specified playlist on the given device (or default device if none specified).
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist to play.</param>
    /// <param name="deviceId">Optional device ID to start playback on.</param>
    public void PlayPlaylist(string playlistId, string? deviceId = null)
    {
        var accessToken = GetAccessToken();
        var uri = "https://api.spotify.com/v1/me/player/play";
        if (!string.IsNullOrEmpty(deviceId)) uri += "?device_id=" + Uri.EscapeDataString(deviceId);

        var body = new { context_uri = $"spotify:playlist:{playlistId}" };
        var json = JsonSerializer.Serialize(body);

        var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
}