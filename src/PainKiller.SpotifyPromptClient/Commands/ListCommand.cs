using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Playlist command",
                      arguments: ["filter"],
                        options: ["update","compare"],
                       examples: ["//View playlist","list","//Update playlists", "list --update","//Compare playlist with updated playlists with tracks","list --compare"])]
public class ListCommand(string identifier) : TracksBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var playlistStorage = new SpotifyObjectStorage<Playlists, PlaylistInfo>();
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
                    var allTracks = PlaylistManager.Default.GetAllTracksForPlaylist(playlist.Id);
                    var playListWithTracks = new PlaylistWithTracks { Id = playlist.Id, Items =  allTracks};
                    playlistTracksStorage.Insert(playListWithTracks, p => p.Id == playListWithTracks.Id, saveToFile: false);
                    Writer.WriteSuccessLine($"Playlists tracks stored for [{playlist.Name}] trackcount: {playlist.TrackCount}");
                    TrackService.Default.StoreTracks(allTracks);
                }
                catch (Exception ex)
                {
                    Writer.WriteError($"Failed to store tracks for [{playlist.Name}] trackcount: {playlist.TrackCount} - {ex.Message}", scope:nameof(ListCommand));
                }
            }
            playlistTracksStorage.Save();
            Writer.WriteSuccessLine("Playlists tracks, albums and artists persisted.");
        }
        if (input.HasOption("compare"))
        {
            var playlists = PlaylistManager.Default.GetAllPlaylists();
            var updated = playlistTracksStorage.GetItems();
            Writer.WriteLine($"Total playlists: {playlists.Count} playlist updated with tracks: {updated.Count}");
            return Ok();
        }
        var storedPlaylists = playlistStorage.GetItems().OrderBy(p => p.Name).ToList();
        var selectedLists = ListService.ShowSelectFromFilteredList<PlaylistInfo>("Select a playlist!", storedPlaylists,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer, filter);
        if (selectedLists.Count == 0) return Ok();
        var selected = ListService.ListDialog("Select playlist", selectedLists.Select(l => l.Name).ToList());
        if (selected.Count == 0) return Ok();
        var selectedId = selectedLists[selected.Keys.First()].Id;
        var selectedPlayList = playlistStorage.GetItems().First(p => p.Id == selectedId);

        var action = ToolbarService.NavigateToolbar<PlayListAction>();
        if (action == PlayListAction.Play)
        {
            PlaylistManager.Default.PlayPlaylist(selectedPlayList.Id);
        }

        if (action == PlayListAction.Play || action == PlayListAction.View)
        {
            var tracks = PlaylistManager.Default.GetAllTracksForPlaylist(selectedPlayList.Id);
            TrackService.Default.UpdateSelectedTracks(tracks);
            ShowSelectedTracks();
        }
        if(action == PlayListAction.Delete)
        {
            var confirm = DialogService.YesNoDialog("Are you sure you want to delete the playlist?");
            if (confirm)
            {
                playlistStorage.Remove(p => p.Id == selectedPlayList.Id, saveToFile: true);
                PlaylistModifyManager.Default.DeletePlaylist(selectedPlayList.Id);
                Writer.WriteSuccessLine($"Playlist [{selectedPlayList.Name}] deleted.");
            }
        }
        return Ok();
    }
    private void Presentation(List<PlaylistInfo> items) => Writer.WriteTable(items);
}