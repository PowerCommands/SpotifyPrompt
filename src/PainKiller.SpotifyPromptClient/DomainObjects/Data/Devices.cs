using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;

public class Devices : IDataObjects<DeviceInfo>
{
    public DateTime LastUpdated { get; set; }
    public List<DeviceInfo> Items { get; set; } = [];
}