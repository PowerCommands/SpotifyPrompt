using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Show album tracks, selected albums will be added to the current selection that could be used to build playlists.\nEnable append mode with append command to append albums to the current selection.", 
                        options: ["year"],
                      arguments: ["filter"],
                       examples: ["//Show album and their tracks","album"])]
public class AlbumCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        input.TryGetOption(out int year, 1955);
        var filter = string.Join(' ', input.Arguments);
        var albumsStorage = new SpotifyObjectStorage<Albums, Album>();
        var albums = albumsStorage.GetItems().Where(a => (a.ReleaseYear == year || year == 1955) && a.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var selectedAlbums = ListService.ShowSelectFromFilteredList("Select a album!", albums,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer, "");
        
        if (selectedAlbums.Count == 0) return Ok();

        if (AppendCommand.AppendMode && selectedAlbums.Count > 1)
        {
            var selectOneAlbum = ListService.ListDialog("Select one album", selectedAlbums.Select(a => $"{a.Artists.FirstOrDefault()?.Name} {a.Name}").ToList());
            if (selectOneAlbum.Count > 0)
            {
                var album = selectedAlbums[selectOneAlbum.First().Key];
                selectedAlbums.Clear();
                selectedAlbums.Add(album);
            }
        }
        if(AppendCommand.AppendMode) SelectedManager.Default.AppendToSelected(selectedAlbums);
        else SelectedManager.Default.UpdateSelected(selectedAlbums);

        ShowSelectedAlbums();
        if (AppendCommand.AppendMode) return Ok();
        
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