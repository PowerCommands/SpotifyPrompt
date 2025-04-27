using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Managers;
public class SelectedManager : ISelectedManager
{
    private readonly List<TrackObject> _selectedTracks = [];
    private SelectedManager() { }
    private static readonly Lazy<ISelectedManager> Instance = new(() => new SelectedManager());
    public static ISelectedManager Default => Instance.Value;

    private readonly ObjectStorage<Tracks, TrackObject> _trackStore = new();
    private readonly SpotifyObjectStorage<Albums, Album> _albumStore = new();
    private readonly SpotifyObjectStorage<Artists, ArtistSimplified> _artistStore = new();
    private readonly List<Album> _selectedAlbums = new();
    private readonly List<ArtistSimplified> _selectedArtists = new();
    public void Store(IEnumerable<TrackObject> tracks)
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
    public void UpdateSelected(List<TrackObject> tracks)
    {
        _selectedTracks.Clear();
        _selectedTracks.AddRange(tracks);
    }
    public void AppendToSelected(List<TrackObject> tracks) => _selectedTracks.AddRange(tracks);
    public List<TrackObject> GetSelectedTracks() => _selectedTracks;

    public void UpdateSelected(List<Album> albums)
    {
        _selectedAlbums.Clear();
        _selectedAlbums.AddRange(albums);
    }
    public void AppendToSelected(List<Album> albums) => _selectedAlbums.AddRange(albums);

    public List<Album> GetSelectedAlbums() => _selectedAlbums;

    public void UpdateSelected(List<ArtistSimplified> artists)
    {
        _selectedArtists.Clear();
        _selectedArtists.AddRange(artists);
    }
    public void UpdateLatestPlaying(TrackObject track, int latestTracksCount)
    {
        var latestPlaying = StorageService<LatestTracks>.Service.GetObject();
        var tracks = latestPlaying.Items.Take(latestTracksCount).ToList();
        if (tracks.Any(t => t.Id == track.Id)) return;
        tracks.Insert(0, track);
        latestPlaying.Items = tracks;
        latestPlaying.LastUpdated = DateTime.Now;
        StorageService<LatestTracks>.Service.StoreObject(latestPlaying);
    }
    public void Clear()
    {
        _selectedTracks.Clear();
        _selectedAlbums.Clear();
        _selectedArtists.Clear();
    }
    public void AppendToSelected(List<ArtistSimplified> artists) => _selectedArtists.AddRange(artists);
    public List<ArtistSimplified> GetSelectedArtists() => _selectedArtists;
}