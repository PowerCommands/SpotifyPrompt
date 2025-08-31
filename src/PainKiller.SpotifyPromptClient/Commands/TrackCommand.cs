using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Show tracks.",
                        options: ["tags"],
                      arguments: ["filter"],
                       examples: ["//Show tracks","track"])]
public class TrackCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        input.TryGetOption(out string tags, "");
        var tracks = tracksStorage.GetItems().Where(info => (info.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || info.Artists.Any(a => a.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))) && info.Tags.Contains(tags, StringComparison.OrdinalIgnoreCase)).ToList();
        CustomListService.ShowSelectedTracks(tracks, Writer);
        return Ok();
    }
}