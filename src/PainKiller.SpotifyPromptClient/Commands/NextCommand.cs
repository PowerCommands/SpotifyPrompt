using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Next command", 
                       examples: ["//Next","next"])]
public class NextCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playerManager = new PlayerManager();
        playerManager.Next();
        return Ok();
    }
}