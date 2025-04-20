namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;
public class Albums : IDataObjects<Album>
{
    public DateTime LastUpdated { get; set; }
    public List<Album> Items { get; set; } = [];
}