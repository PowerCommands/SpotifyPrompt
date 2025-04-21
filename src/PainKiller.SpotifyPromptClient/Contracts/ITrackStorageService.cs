namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ITrackService
{
    void StoreTracks(IEnumerable<TrackObject> tracks);
    void UpdateSelectedTracks(List<TrackObject> tracks);
    void AppendToSelectedTracks(List<TrackObject> tracks);
    List<TrackObject> GetSelectedTracks();
}