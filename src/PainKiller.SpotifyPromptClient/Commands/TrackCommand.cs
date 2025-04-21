using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Show tracks.", 
                      arguments: ["filter"],
                       examples: ["//Show tracks","track"])]
public class TrackCommand(string identifier) : TracksBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = tracksStorage.GetItems();
        var selectedTracks = ListService.ShowSelectFromFilteredList<TrackObject>("Select a track!", tracks,(info, s) => (info.Name.Contains(s,StringComparison.OrdinalIgnoreCase) || info.Artists.Any(a => a.Name.Contains(s, StringComparison.OrdinalIgnoreCase))), Presentation, Writer, filter);
        TrackService.Default.UpdateSelectedTracks(selectedTracks);
        ShowSelectedTracks();
        return Ok();
    }
    private void Presentation(List<TrackObject> items) => Writer.WriteTable(items.Select(a => new{Name = a.Name, Artist = a.Artists.FirstOrDefault()?.Name}));
}