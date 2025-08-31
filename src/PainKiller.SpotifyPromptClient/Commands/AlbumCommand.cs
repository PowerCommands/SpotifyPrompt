using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "SpotifyPrompt - Show album tracks", 
                        options: ["year"],
                         quotes: ["filter"],
                       examples: ["//Show album with name \"powerslave\"","album \"powerslave\""])]
public class AlbumCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        input.TryGetOption(out int year, 1955);
        var filter = string.Join(' ', input.Arguments);
        var albumsStorage = new SpotifyObjectStorage<Albums, Album>();
        var albums = albumsStorage.GetItems().Where(a => (a.ReleaseYear == year || year == 1955) && a.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var selectedAlbums = CustomListService.ShowSelectFromList("Select a album!", albums, Writer, true, (info, s) => info.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
        
        if (selectedAlbums.Count == 0) return Ok();
        
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var album in selectedAlbums)
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Album.Name.Contains(album.Name)).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        CustomListService.ShowSelectedTracks(tracks, Writer);
        return Ok();
    }
}