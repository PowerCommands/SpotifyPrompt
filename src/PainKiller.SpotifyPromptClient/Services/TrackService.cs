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

        var nowPlayingResp = _http.SendAsync(nowPlayingReq).GetAwaiter().GetResult();
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

        var trackResp = _http.SendAsync(trackReq).GetAwaiter().GetResult();
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
}