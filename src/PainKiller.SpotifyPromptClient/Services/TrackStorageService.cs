using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Services;
public class TrackStorageService : ITrackStorageService
{
    private TrackStorageService() { }
    private static readonly Lazy<ITrackStorageService> Instance = new(() => new TrackStorageService());
    public static ITrackStorageService Default => Instance.Value;

    private readonly ObjectStorage<Tracks, TrackObject> _trackStore = new();
    private readonly ObjectStorage<Albums, Album> _albumStore = new();
    private readonly ObjectStorage<Artists, ArtistSimplified> _artistStore = new();
    public void StoreTracks(IEnumerable<TrackObject> tracks)
    {
        var uniqueTracks = tracks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
        var uniqueAlbums = uniqueTracks.Select(t => t.Album).GroupBy(a => a.Id).Select(g => g.First());
        var uniqueArtists = uniqueTracks.SelectMany(t => t.Artists).GroupBy(a => a.Id).Select(g => g.First());

        foreach (var tr in uniqueTracks) _trackStore.Insert(tr, t => t.Id == tr.Id, saveToFile: false);
        foreach (var al in uniqueAlbums) _albumStore.Insert(al, a => a.Id == al.Id, saveToFile: false);
        foreach (var ar in uniqueArtists) _artistStore.Insert(ar, a => a.Id == ar.Id, saveToFile: false);

        _trackStore.Save();
        _albumStore.Save();
        _artistStore.Save();
    }
}