using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View latest played tracks (while running this client)", 
                       examples: ["//View latest played tracks","latest"])]
public class LatestCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var latest = LatestService.Default.GetLatestTracks();
        Writer.WriteDescription("Latest played tracks (while client running):", latest.Count.ToString());
        SelectedService.Default.UpdateSelected(latest);
        ShowSelectedTracks();
        var action = ToolbarService.NavigateToolbar<LatestAction>(title:"What do you want to do with the history?");
        if (action == LatestAction.Nothing) return Ok();
        if (action == LatestAction.Clear)
        {
            LatestService.Default.Clear();
            Writer.WriteSuccessLine("Latest track history cleared");
            return Ok();
        }
        Writer.WriteLine();
        var playlistName = DialogService.QuestionAnswerDialog("Name of the playlist:");
        var tag = ToolbarService.NavigateToolbar<Genres>(title: "Select a tag");
        PlaylistService.Default.CreatePlaylist(playlistName, $"Playlist created with {Configuration.Core.Name}", latest, [tag.ToString()]);
        Writer.WriteSuccessLine($"Playlist [{playlistName}] created.");
        return Ok();
    }
}