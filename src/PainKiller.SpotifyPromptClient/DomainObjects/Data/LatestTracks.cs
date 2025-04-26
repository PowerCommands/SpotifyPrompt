namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;

public class LatestTracks : IDataObjects<TrackObject>
{
    public DateTime LastUpdated { get; set; }
    public List<TrackObject> Items { get; set; } = [];
}