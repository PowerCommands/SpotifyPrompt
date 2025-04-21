using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Shuffle command", 
                       examples: ["//Enable/Disable Shuffle","shuffle"])]
public class ShuffleCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerManager playerManager = new PlayerManager();
        var shuffleState = playerManager.GetShuffleState();
        playerManager.SetShuffle(!shuffleState);
        Writer.WriteSuccessLine(shuffleState ? "Shuffle is now disabled" : "Shuffle is now enabled");
        return Ok();
    }
}