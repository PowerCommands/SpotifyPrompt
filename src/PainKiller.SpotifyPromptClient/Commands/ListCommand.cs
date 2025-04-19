using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.DomainObjects;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Playlist command", 
                        options: ["update"],
                       examples: ["//View playlist","list","//Update playlists", "list --update"])]
public class ListCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var storage = new ObjectStorage<Playlists, PlaylistInfo>();
        if (input.HasOption("update"))
        {
            var playlists = PlaylistManager.Default.GetAllPlaylists();
            storage.SaveItems(playlists);
            Writer.WriteSuccessLine("Playlists stored, now continuing with the tracks");
            foreach (var playlist in playlists)
            {
                
            }
        }
        var storedPlaylists = storage.GetItems().OrderBy(p => p.Name).ToList();
        var selected = ListService.ShowSelectFromFilteredList<PlaylistInfo>("Select a playlist!", storedPlaylists,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer);
        if (selected.Count == 0) return Ok();
        PlaylistManager.Default.PlayPlaylist(selected.First().Id);
        var tracks = PlaylistManager.Default.GetAllTracksForPlaylist(selected.First().Id);
        Writer.WriteTable(tracks.Select(t => new{Artist = t.Artists.First().Name,Title = t.Name ,Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4," ")}));
        return Ok();
    }

    private void Presentation(List<PlaylistInfo> items)
    {
        Writer.WriteTable(items);
    }
}