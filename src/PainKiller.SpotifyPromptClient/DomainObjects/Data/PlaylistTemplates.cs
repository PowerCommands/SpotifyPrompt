namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;
public class PlaylistTemplates : IDataObjects<PlaylistTemplate>
{
    public DateTime LastUpdated { get; set; }
    public List<PlaylistTemplate> Items { get; set; } = [new() { Id = "", Name = "Create new" } , new()];
}