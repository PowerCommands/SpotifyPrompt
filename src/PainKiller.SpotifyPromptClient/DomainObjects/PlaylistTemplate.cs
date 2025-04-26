using PainKiller.SpotifyPromptClient.Enums;

namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class PlaylistTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public PlaylistSourceType SourceType { get; set; } = PlaylistSourceType.Tracks;
    public RandomMode RandomMode { get; set; } = RandomMode.All;
    public string Name { get; set; } = "Default";
    public string Description => $"{Count} {string.Join(' ', Tags)} tracks with year range {YearRange} the  from your playlists";
    public List<string> Tags { get; set; } = ["pop"];
    public int Count { get; set; } = 100;
    public YearRange YearRange { get; set; } = new(1980, 1989);
    public bool UniqueArtists { get; set; }
}