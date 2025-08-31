namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class ArtistSimplified : IContainsTags
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public override string ToString() => Name;
}