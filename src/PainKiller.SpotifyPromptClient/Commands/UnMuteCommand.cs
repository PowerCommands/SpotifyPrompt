using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Un Mute the Spotify player, could also be used to set the volume with the --volume option flag.", 
                        options: ["volume"],
                       examples: ["//Un mute the volume","unmute", "//Un mute and set the volume to 50%","unmute --volume 50"])]
public class UnMuteCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var volume = input.GetTypedOptionValue<int>("volume", "100");
        DeviceService.Default.SetVolume(volume);
        return Ok();
    }
}