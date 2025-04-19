
namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class PlaylistWithTracks
{
    public string Id { get; set; } = string.Empty;
    public List<TrackObject> Items { get; set; } = [];
}