using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Contracts;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Player command", 
                        options: [""],
                       examples: ["//Play","play"])]
public class PlayCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized() => ShellService.Default.Execute("spotify");
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerManager playerManager = new PlayerManager(RefreshTokenManager.DefaultInstance());
        playerManager.Play();
        return Ok();
    }
}