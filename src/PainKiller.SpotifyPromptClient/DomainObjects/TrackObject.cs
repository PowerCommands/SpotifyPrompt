namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class TrackObject : IContainsTags
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ArtistSimplified> Artists { get; set; } = [];
    public Album Album { get; set; } = new Album();
    public int DurationMs { get; set; }
    public string Uri { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}