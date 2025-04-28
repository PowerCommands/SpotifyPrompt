using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Handle the volume on the current active device.", 
                        options: ["volume"],
                       examples: ["//Set volume to 80%","volume 80"])]
public class VolumeCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        int.TryParse(input.Arguments.FirstOrDefault(), out var volume);
        var currentVolume = DeviceService.Default.GetCurrentVolume();
        if(!(volume == 0 || volume == currentVolume))
        {
            DeviceService.Default.SetVolume(volume);
        }
        else volume = currentVolume;
        Writer.WriteLine($"Current volume: {volume}%");
        return Ok();
    }
}