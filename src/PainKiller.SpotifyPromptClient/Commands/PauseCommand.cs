using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Paus command", 
                       examples: ["//Pause","pause"])]
public class PauseCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playerManager = new PlayerManager();
        playerManager.Pause();
        return Ok();
    }
}