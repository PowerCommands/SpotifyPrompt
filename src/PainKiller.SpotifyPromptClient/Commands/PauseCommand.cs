using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Paus the current active player.", 
                       examples: ["//Pause","pause"])]
public class PauseCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playerManager = new PlayerService();
        playerManager.Pause();
        return Ok();
    }
}