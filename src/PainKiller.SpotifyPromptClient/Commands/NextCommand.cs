using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Play next track in the queue.", 
                       examples: ["//Next","next"])]
public class NextCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playerManager = new PlayerService();
        playerManager.Next();
        return Ok();
    }
}