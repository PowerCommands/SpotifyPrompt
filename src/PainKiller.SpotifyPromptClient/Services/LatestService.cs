using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Services;
public class LatestService : ILatestService
{
    private LatestService() { }
    private static readonly Lazy<ILatestService> Instance = new(() => new LatestService());
    public static ILatestService Default => Instance.Value;

    public void UpdateLatest(TrackObject? track, int latestTracksCount)
    {
        if (track == null) return;
        var latestPlaying = StorageService<LatestTracks>.Service.GetObject();
        var tracks = latestPlaying.Items.Take(latestTracksCount).ToList();
        if (tracks.Any(t => t.Id == track.Id)) return;
        tracks.Insert(0, track);
        latestPlaying.Items = tracks;
        latestPlaying.LastUpdated = DateTime.Now;
        StorageService<LatestTracks>.Service.StoreObject(latestPlaying);
    }
    public List<TrackObject> GetLatestTracks() => StorageService<LatestTracks>.Service.GetObject().Items;

    public void Clear()
    {
        var tracks = new List<TrackObject>();
        var latestTracks = new LatestTracks() { Items = tracks };
        StorageService<LatestTracks>.Service.StoreObject(latestTracks);
    }
}