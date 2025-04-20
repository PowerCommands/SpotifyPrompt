namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ITrackStorageService
{
    void StoreTracks(IEnumerable<TrackObject> tracks);
}