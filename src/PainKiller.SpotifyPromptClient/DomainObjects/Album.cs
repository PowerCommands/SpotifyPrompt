namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class Album : IContainsTags
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ArtistSimplified> Artists { get; set; } = [];
    public string ReleaseDate { get; set; } = string.Empty;
    public int TotalTracks { get; set; }
    public string Uri { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}