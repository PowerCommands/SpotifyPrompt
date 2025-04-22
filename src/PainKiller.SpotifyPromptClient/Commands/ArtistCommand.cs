using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Artist actions", 
                      arguments: ["filter"],
                        options: ["tags"],
                       examples: ["//Show your artists and their tracks","artist"])]
public class ArtistCommand(string identifier) : TracksBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var artistStorage = new SpotifyObjectStorage<Artists, ArtistSimplified>();
        var artists = artistStorage.GetItems();
        var tags = input.GetOptionValue("tags");
        var selectedArtists = ListService.ShowSelectFromFilteredList("Select a artist!", artists,(info, s) => (info.Name.Contains(s,StringComparison.OrdinalIgnoreCase) && info.Tags.Contains(tags, StringComparison.OrdinalIgnoreCase)), Presentation, Writer, filter);
        if (selectedArtists.Count == 0) return Ok();

        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var artist in selectedArtists.Take(10))
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Artists.Any(a => a.Name.Contains(artist.Name))).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        TrackService.Default.UpdateSelectedTracks(tracks);
        
        ShowSelectedTracks();
        return Ok();
    }
    private void Presentation(List<ArtistSimplified> items) => Writer.WriteTable(items);
}