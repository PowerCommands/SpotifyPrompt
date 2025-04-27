namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ILatestManager
{
    void UpdateLatest(TrackObject? track, int latestTracksCount);
    List<TrackObject> GetLatestTracks();
    void Clear();
}