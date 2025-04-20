using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;
namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;
public class Tracks : IDataObjects<TrackObject>
{
    public DateTime LastUpdated { get; set; }
    public List<TrackObject> Items { get; set; } = [];
}