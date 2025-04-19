using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;

public class Playlists : IDataObjects<PlaylistInfo>
{
    public DateTime LastUpdated { get; set; }
    public List<PlaylistInfo> Items { get; set; } = [];
}