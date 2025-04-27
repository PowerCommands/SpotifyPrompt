using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Show album tracks.", 
                      arguments: ["filter"],
                       examples: ["//Show album and their tracks","album"])]
public class AlbumCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var albumsStorage = new SpotifyObjectStorage<Albums, Album>();
        var albums = albumsStorage.GetItems();
        var selectedAlbums = ListService.ShowSelectFromFilteredList("Select a album!", albums,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer, filter);
        
        if (selectedAlbums.Count == 0) return Ok();

        SelectedManager.Default.UpdateSelected(selectedAlbums);
        
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var album in selectedAlbums.Take(10))
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Album.Name.Contains(album.Name)).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        SelectedManager.Default.UpdateSelected(tracks);
        
        ShowSelectedTracks();
        return Ok();
    }
    private void Presentation(List<Album> items) => Writer.WriteTable(items.Select(a => new{Name = a.Name, Artist = a.Artists.FirstOrDefault()?.Name, ReleaseDate = a.ReleaseDate, TotalTracks = a.TotalTracks}));
}