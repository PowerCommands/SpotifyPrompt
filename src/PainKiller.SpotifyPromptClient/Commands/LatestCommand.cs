using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View latest played tracks (while running this client)", 
                       examples: ["//View latest played tracks","latest"])]
public class LatestCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var queue = LatestService.Default.GetLatestTracks();
        Writer.WriteTable(queue.Select(t => new{Artist = t.Artists.First().Name,Title = t.Name ,Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4,"")}));
        return Ok();
    }
}