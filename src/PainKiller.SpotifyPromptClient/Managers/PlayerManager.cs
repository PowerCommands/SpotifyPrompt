﻿using System.Net.Http.Headers;
using System.Text.Json;
using PainKiller.SpotifyPromptClient.BaseClasses;

namespace PainKiller.SpotifyPromptClient.Managers;

/// <summary>
/// Wraps playback control calls to Spotify Web API, using the currently stored access token.
/// Attempts to select an active device; if none is active, picks the first available device.
/// Assumes token is kept fresh by InfoPanel refresh thread.
/// </summary>
public class PlayerManager(RefreshTokenManager refreshTokenManager) : SpotifyClientBase(refreshTokenManager)
{
    private string GetDeviceId()
    {
        var accessToken = GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        var root = doc.RootElement;
        var devices = root.GetProperty("devices");

        // Try active device first
        var active = devices.EnumerateArray()
            .FirstOrDefault(d => d.GetProperty("is_active").GetBoolean());
        if (active.ValueKind != JsonValueKind.Undefined)
            return active.GetProperty("id").GetString()!;

        // Otherwise pick first available
        var first = devices.EnumerateArray().FirstOrDefault();
        if (first.ValueKind != JsonValueKind.Undefined)
            return first.GetProperty("id").GetString()!;

        throw new InvalidOperationException("No available Spotify devices found.");
    }

    private void SendCommand(HttpMethod method, string endpoint, object? content = null)
    {
        var accessToken = GetAccessToken();
        var deviceId = GetDeviceId();
        var url = $"{endpoint}?device_id={deviceId}";
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        var response = _http.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
    public void Play()
    {
        SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play");
    }
    public void Pause()
    {
        SendCommand(HttpMethod.Put, "https://api.spotify.com/v1/me/player/pause");
    }
    public void Next()
    {
        SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next");
    }
    public void Previous()
    {
        SendCommand(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous");
    }
}