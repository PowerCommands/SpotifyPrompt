using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;

public class Artists : IDataObjects<ArtistSimplified>
{
    public DateTime LastUpdated { get; set; }
    public List<ArtistSimplified> Items { get; set; } = [];
}