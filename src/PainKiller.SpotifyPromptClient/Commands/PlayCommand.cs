using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Start play on the active Spotify player.",
                      arguments: ["index"],
                        options: ["all"],
                       examples: ["//Play","play","//Play the third track of the current selected tracks.","play 3","//Play all selected tracks","play --all"])]
public class PlayCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        if(Configuration.Spotify.StartSpotifyClient) ShellService.Default.Execute("spotify");
    }
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerService playerManager = new PlayerService();
        var index = input.Arguments.Length > 0 ? int.Parse(input.Arguments[0]) : -1;
        var selectedTracks = SelectedManager.Default.GetSelectedTracks();
        if (input.HasOption("all") && selectedTracks.Count > 0)
        {
            playerManager.Play(selectedTracks.Select(t => t.Uri));
            return Ok();
        }
        if (index < 1)
        {
            playerManager.Play();
            return Ok();
        }
        if (index < selectedTracks.Count)
        {
            playerManager.Play(selectedTracks[index-1].Uri);
            return Ok();
        }
        playerManager.Play();
        return Ok();
    }
}