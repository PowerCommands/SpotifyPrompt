namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;
public class PlaylistTracks : IDataObjects<PlaylistWithTracks>
{
    public DateTime LastUpdated { get; set; }
    public List<PlaylistWithTracks> Items { get; set; } = [];
}