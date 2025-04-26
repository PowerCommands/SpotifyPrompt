namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ILatestService
{
    void UpdateLatest(TrackObject? track, int latestTracksCount);
    List<TrackObject> GetLatestTracks();
    void Clear();
}