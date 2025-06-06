using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View your devices that could be controlled by Spotify Prompt using the WebAPI, only devices that are using the WebAPI are shown.", 
                       examples: ["//Show devices that have been registered by Spotify Prompt","device"])]
public class DeviceCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var storage = new ObjectStorage<Devices, DeviceInfo>();
        if (input.Arguments.Length > 0)
        {
            var deviceName = input.Arguments.First();
            var device = storage.GetItems().FirstOrDefault(d => d.Name == deviceName);
            if (device != null) DeviceService.Default.SetActiveDevice(device.Id);
        }
        var devices = DeviceService.Default.GetDevices();
        foreach (var deviceInfo in devices) storage.Insert(deviceInfo, info => info.Id == deviceInfo.Id);
        var storedDevices = storage.GetItems();
        Writer.WriteTable(storedDevices);
        return Ok();
    }
}