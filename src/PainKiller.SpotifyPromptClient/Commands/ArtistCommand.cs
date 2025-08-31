using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Artist actions, selected artists will be stored in the current selection that could be used to build playlists.\nEnable append mode with append command to append artists to the current selection.", 
                      arguments: ["filter"],
                        options: ["tags"],
                       examples: ["//Show your artists and their tracks","artist"])]
public class ArtistCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var albumsStorage = new SpotifyObjectStorage<Artists, ArtistSimplified>();
        var albums = albumsStorage.GetItems().Where(a => a.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var selectedArtists = CustomListService.ShowSelectFromList("Select artist(s)!", albums, Writer, true, (info, s) => info.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
        
        if (selectedArtists.Count == 0) return Ok();
        
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var artist in selectedArtists)
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Artists.Any(art => art.Name.Contains(artist.Name))).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        CustomListService.ShowSelectedTracks(tracks, Writer);
        return Ok();
    }
}