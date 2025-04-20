using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Player command", 
                       examples: ["//Play","play"])]
public class PlayCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized() => ShellService.Default.Execute("spotify");
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerManager playerManager = new PlayerManager();
        playerManager.Play();
        return Ok();
    }
}