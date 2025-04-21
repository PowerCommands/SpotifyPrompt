using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Services;
public class TrackService : ITrackService
{
    private readonly List<TrackObject> _selected = [];
    private TrackService() { }
    private static readonly Lazy<ITrackService> Instance = new(() => new TrackService());
    public static ITrackService Default => Instance.Value;

    private readonly ObjectStorage<Tracks, TrackObject> _trackStore = new();
    private readonly SpotifyObjectStorage<Albums, Album> _albumStore = new();
    private readonly SpotifyObjectStorage<Artists, ArtistSimplified> _artistStore = new();
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
    public void UpdateSelectedTracks(List<TrackObject> tracks)
    {
        _selected.Clear();
        _selected.AddRange(tracks);
    }
    public void AppendToSelectedTracks(List<TrackObject> tracks) => _selected.AddRange(tracks);
    public List<TrackObject> GetSelectedTracks() => _selected;
}