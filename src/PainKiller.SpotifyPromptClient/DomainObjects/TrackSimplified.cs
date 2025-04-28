namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class TrackSimplified
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; }= string.Empty;
    public string Uri { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public int TrackNumber { get; set; }
}