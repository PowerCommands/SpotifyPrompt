namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class PlaylistInfo : IContainsTags
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public string Tags { get; set; } = string.Empty;
}