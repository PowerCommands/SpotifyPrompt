namespace PainKiller.SpotifyPromptClient.Configuration;
public class SpotifyConfiguration
{
    public string RedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = [];
    public int RefreshMarginInMinutes { get; set; } = 5;
}