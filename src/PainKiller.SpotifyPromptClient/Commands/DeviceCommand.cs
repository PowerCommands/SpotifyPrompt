using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.SpotifyPromptClient.Configuration;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Player command", 
                       examples: ["//Play","play"])]
public class DeviceCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        return Ok();
    }
}