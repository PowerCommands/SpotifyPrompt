namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;
public class RelatedArtists : IDataObjects<RelatedArtist>
{
    public DateTime LastUpdated { get; set; }
    public List<RelatedArtist> Items { get; set; } = [];
}