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
                        options: ["update","compare"],
                       examples: ["//View playlist","list","//Update playlists", "list --update","//Compare playlist with updated playlists with tracks","list --update"])]
public class ListCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var playlistStorage = new ObjectStorage<Playlists, PlaylistInfo>();
        var playlistTracksStorage = new ObjectStorage<PlaylistTracks, PlaylistWithTracks>();

        if (input.HasOption("update"))
        {
            var playlists = PlaylistManager.Default.GetAllPlaylists();
            playlistStorage.SaveItems(playlists);
            Writer.WriteSuccessLine("Playlists stored, now continuing with the tracks");

            var playlistTracks = playlistTracksStorage.GetItems();
            foreach (var playlist in playlists)
            {
                try
                {
                    var existing = playlistTracks.FirstOrDefault(p => p.Id == playlist.Id);
                    if(existing != null && playlist.TrackCount == existing.Items.Count) continue;
                    var playListWithTracks = new PlaylistWithTracks { Id = playlist.Id, Items = PlaylistManager.Default.GetAllTracksForPlaylist(playlist.Id) };
                    playlistTracksStorage.Insert(playListWithTracks, p => p.Id == playListWithTracks.Id);
                    Writer.WriteSuccessLine($"Playlists tracks stored for [{playlist.Name}] trackcount: {playlist.TrackCount}");
                }
                catch (Exception ex)
                {
                    Writer.WriteError($"Failed to store tracks for [{playlist.Name}] trackcount: {playlist.TrackCount} - {ex.Message}");
                }
            }
        }
        if (input.HasOption("compare"))
        {
            var playlists = PlaylistManager.Default.GetAllPlaylists();
            var updated = playlistTracksStorage.GetItems();
            Writer.WriteLine($"Total playlists: {playlists.Count} playlist updated with tracks: {updated.Count}");
            return Ok();
        }
        var storedPlaylists = playlistStorage.GetItems().OrderBy(p => p.Name).ToList();
        var selected = ListService.ShowSelectFromFilteredList<PlaylistInfo>("Select a playlist!", storedPlaylists,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer);
        if (selected.Count == 0) return Ok();
        PlaylistManager.Default.PlayPlaylist(selected.First().Id);
        var tracks = PlaylistManager.Default.GetAllTracksForPlaylist(selected.First().Id);
        Writer.WriteTable(tracks.Select(t => new{Artist = t.Artists.First().Name,Title = t.Name ,Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4," ")}));
        return Ok();
    }
    private void Presentation(List<PlaylistInfo> items) => Writer.WriteTable(items);
}