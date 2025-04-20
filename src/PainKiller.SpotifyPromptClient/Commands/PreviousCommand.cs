using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Previous command", 
                       examples: ["//Previous","previous"])]
public class PreviousCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playerManager = new PlayerManager();
        playerManager.Previous();
        return Ok();
    }
}