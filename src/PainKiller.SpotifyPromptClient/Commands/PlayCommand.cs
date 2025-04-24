using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Player command",
                      arguments: ["index"],
                       examples: ["//Play","play","//Play the third track of the current selected tracks.","play 3"])]
public class PlayCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        if(Configuration.Spotify.StartSpotifyClient) ShellService.Default.Execute("spotify");
    }
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerManager playerManager = new PlayerManager();
        var index = input.Arguments.Length > 0 ? int.Parse(input.Arguments[0]) : -1;
        if (index < 1)
        {
            playerManager.Play();
            return Ok();
        }
        var selectedTracks = SelectedService.Default.GetSelectedTracks();
        if (index < selectedTracks.Count)
        {
            playerManager.Play(selectedTracks[index-1].Uri);
            return Ok();
        }
        playerManager.Play();
        return Ok();
    }
}