using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Turn shuffle on or of depending on the current state (on will be off when running this command and vice versa).", 
                       examples: ["//Enable/Disable Shuffle","shuffle"])]
public class ShuffleCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerService playerManager = new PlayerService();
        var shuffleState = playerManager.GetShuffleState();
        playerManager.SetShuffle(!shuffleState);
        Writer.WriteSuccessLine(shuffleState ? "Shuffle is now disabled" : "Shuffle is now enabled");
        return Ok();
    }
}