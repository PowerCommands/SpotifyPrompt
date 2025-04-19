using PainKiller.SpotifyPromptClient.DomainObjects;
using System.Net.Http.Headers;
using System.Text.Json;
using PainKiller.SpotifyPromptClient.BaseClasses;
using PainKiller.SpotifyPromptClient.Contracts;

namespace PainKiller.SpotifyPromptClient.Managers;

public class QueueManager : SpotifyClientBase, IQueueManager
{
    private QueueManager(){}
    private static readonly Lazy<IQueueManager> Instance = new(() => new QueueManager());
    public static IQueueManager Default => Instance.Value;

    public List<TrackObject> GetQueue()
    {
        var accessToken = GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/queue");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        var queueArray = doc.RootElement.GetProperty("queue").EnumerateArray();

        return queueArray.Select(t => new TrackObject
        {
            Id = t.GetProperty("id").GetString() ?? "",
            Name = t.GetProperty("name").GetString() ?? "",
            DurationMs = t.GetProperty("duration_ms").GetInt32(),
            Uri = t.GetProperty("uri").GetString() ?? "",
            Artists = t.GetProperty("artists")
                       .EnumerateArray()
                       .Select(a => new ArtistSimplified {
                           Id = a.GetProperty("id").GetString() ?? "",
                           Name = a.GetProperty("name").GetString() ?? "",
                           Uri = a.GetProperty("uri").GetString() ?? ""
                       }).ToList(),
            Album = new Album
            {
                Id = t.GetProperty("album").GetProperty("id").GetString() ?? "",
                Name = t.GetProperty("album").GetProperty("name").GetString() ?? "",
                ReleaseDate = t.GetProperty("album").GetProperty("release_date").GetString() ?? "",
                TotalTracks = t.GetProperty("album").GetProperty("total_tracks").GetInt32(),
                Uri = t.GetProperty("album").GetProperty("uri").GetString() ?? "",
                Artists = t.GetProperty("album").GetProperty("artists")
                           .EnumerateArray()
                           .Select(a => new ArtistSimplified {
                               Id = a.GetProperty("id").GetString() ?? "",
                               Name = a.GetProperty("name").GetString() ?? "",
                               Uri = a.GetProperty("uri").GetString() ?? ""
                           }).ToList()
            }
        }).ToList();
    }
    public void AddToQueue(string trackUri, string? deviceId = null)
    {
        var accessToken = GetAccessToken();
        var url = $"https://api.spotify.com/v1/me/player/queue?uri={Uri.EscapeDataString(trackUri)}";
        if (!string.IsNullOrEmpty(deviceId))
            url += $"&device_id={Uri.EscapeDataString(deviceId)}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
}