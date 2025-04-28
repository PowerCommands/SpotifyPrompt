using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Mute command", 
                       examples: ["//Mute the volyme","mute"])]
public class MuteCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        DeviceService.Default.SetVolume(0);
        return Ok();
    }
}