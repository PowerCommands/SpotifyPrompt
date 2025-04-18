﻿namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class PlaylistInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public int TrackCount { get; set; }
}