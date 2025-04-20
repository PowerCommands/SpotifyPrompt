using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Shuffle command", 
                       examples: ["//Enable/Disable Shuffle","shuffle"])]
public class ShuffleCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized() => ShellService.Default.Execute("spotify");
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerManager playerManager = new PlayerManager();
        var shuffleState = playerManager.GetShuffleState();
        playerManager.SetShuffle(!shuffleState);
        Writer.WriteSuccessLine(shuffleState ? "Shuffle is now disabled" : "Shuffle is now enabled");
        return Ok();
    }
}