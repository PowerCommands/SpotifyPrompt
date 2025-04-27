namespace PainKiller.SpotifyPromptClient.Configuration;
public class SpotifyConfiguration
{
    public string RedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = [];
    public int RefreshMarginInMinutes { get; set; } = 5;
    public bool StartSpotifyClient { get; set; }
    public int LatestTracksCount { get; set; } = 1000;
    public string PlaylistNamePrefix { get; set; } = "SPC";
}