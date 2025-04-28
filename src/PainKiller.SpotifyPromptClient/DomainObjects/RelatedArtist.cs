namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class RelatedArtist : IDataObjects<ArtistSimplified>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<ArtistSimplified> Items { get; set; } = [];
}