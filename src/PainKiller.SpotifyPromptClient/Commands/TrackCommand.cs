using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Show tracks.",
                        options: ["tags"],
                      arguments: ["filter"],
                       examples: ["//Show tracks","track"])]
public class TrackCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        input.TryGetOption(out string tags, "");
        var tracks = tracksStorage.GetItems();
        var selectedTracks = ListService.ShowSelectFromFilteredList<TrackObject>("Select a track!", tracks,(info, s) => ((info.Name.Contains(s,StringComparison.OrdinalIgnoreCase) || info.Artists.Any(a => a.Name.Contains(s, StringComparison.OrdinalIgnoreCase))) && info.Tags.Contains(tags, StringComparison.OrdinalIgnoreCase)), Presentation, Writer, filter);
        SelectedManager.Default.UpdateSelected(selectedTracks);
        ShowSelectedTracks();
        return Ok();
    }
    private void Presentation(List<TrackObject> items) => Writer.WriteTable(items.Select(a => new{Name = a.Name, Artist = a.Artists.FirstOrDefault()?.Name, Tags = a.Tags}));
}