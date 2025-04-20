using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Player command", 
                       examples: ["//Play","play"])]
public class DeviceCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var storage = new ObjectStorage<Devices, DeviceInfo>();
        if (input.Arguments.Length > 0)
        {
            var deviceName = input.Arguments.First();
            var device = storage.GetItems().FirstOrDefault(d => d.Name == deviceName);
            if (device != null) DeviceManager.Default.SetActiveDevice(device.Id);
        }
        var devices = DeviceManager.Default.GetDevices();
        foreach (var deviceInfo in devices) storage.Insert(deviceInfo, info => info.Id == deviceInfo.Id);
        var storedDevices = storage.GetItems();
        Writer.WriteTable(storedDevices);
        return Ok();
    }
}