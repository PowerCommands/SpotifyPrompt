using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.SpotifyPromptClient.Services;
public class TrackService : SpotifyClientBase, ITrackService
{
    private readonly ILogger<TrackService> _logger = LoggerProvider.CreateLogger<TrackService>();

    private TrackService() { }
    private static readonly Lazy<ITrackService> Instance = new(() => new TrackService());
    public static ITrackService Default => Instance.Value;

    public TrackObject? GetCurrentlyPlayingTrack()
    {
        var accessToken = GetAccessToken();
        var nowPlayingReq = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/currently-playing");
        nowPlayingReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var nowPlayingResp = Http.SendAsync(nowPlayingReq).GetAwaiter().GetResult();
        _logger.LogInformation($"GetCurrentlyPlaying: {nowPlayingResp.StatusCode}");
        if (nowPlayingResp.StatusCode == HttpStatusCode.NoContent)
            return null;

        nowPlayingResp.EnsureSuccessStatusCode();
        using var nowJson = JsonDocument.Parse(nowPlayingResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        // Only proceed if it's a track (could be episode, etc.)
        var playingType = nowJson.RootElement.GetProperty("currently_playing_type").GetString();
        if (!string.Equals(playingType, "track", StringComparison.OrdinalIgnoreCase))
            return null;

        var trackId = nowJson.RootElement
            .GetProperty("item")
            .GetProperty("id")
            .GetString();

        if (string.IsNullOrEmpty(trackId)) return null;
        
        return GetTrack(trackId);
    }
    public TrackObject GetTrack(string trackId)
    {
        var accessToken = GetAccessToken();
        var trackReq = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/tracks/{trackId}");
        trackReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var trackResp = Http.SendAsync(trackReq).GetAwaiter().GetResult();
        trackResp.EnsureSuccessStatusCode();
        using var trackJson = JsonDocument.Parse(trackResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        var root = trackJson.RootElement;
        var to = new TrackObject
        {
            Id         = root.GetProperty("id").GetString()!,
            Name       = root.GetProperty("name").GetString()!,
            Uri        = root.GetProperty("uri").GetString()!,
            DurationMs = root.GetProperty("duration_ms").GetInt32(),
            // Artists
            Artists    = root
                .GetProperty("artists")
                .EnumerateArray()
                .Select(a => new ArtistSimplified
                {
                    Id   = a.GetProperty("id").GetString()!,
                    Name = a.GetProperty("name").GetString()!
                })
                .ToList()
        };
        
        var alb = root.GetProperty("album");
        to.Album = new Album
        {
            Id          = alb.GetProperty("id").GetString()!,
            Name        = alb.GetProperty("name").GetString()!,
            ReleaseDate = alb.GetProperty("release_date").GetString()!
        };
        return to;
    }

    public List<TrackObject> GetAlbumTracks(string albumId)
    {
        var accessToken = GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/albums/{albumId}/tracks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = Http.SendAsync(request).GetAwaiter().GetResult();
        _logger.LogInformation($"GetAlbumTracks: {response.StatusCode} för album {albumId}");
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items").EnumerateArray();
        var tracks = items
            .Select(item => new TrackObject
            {
                Id         = item.GetProperty("id").GetString()!,
                Name       = item.GetProperty("name").GetString()!,
                Uri        = item.GetProperty("uri").GetString()!,
                DurationMs = item.GetProperty("duration_ms").GetInt32(),
                Artists    = item
                    .GetProperty("artists")
                    .EnumerateArray()
                    .Select(a => new ArtistSimplified
                    {
                        Id   = a.GetProperty("id").GetString()!,
                        Name = a.GetProperty("name").GetString()!
                    })
                    .ToList(),
                Album = new Album
                {
                    Id = albumId
                }
            })
            .ToList();
        return tracks;
    }
}