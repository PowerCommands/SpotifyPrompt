using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.BaseClasses;

public abstract class TracksBaseCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    protected void ShowSelectedTracks()
    {
        var tracks = TrackService.Default.GetSelectedTracks();
        if (tracks.Count == 0)
        {
            Writer.WriteLine("No tracks selected.");
            return;
        }
        var action = ToolbarService.NavigateToolbar<SelectedTracksAction>();
        if (action == SelectedTracksAction.Queue)
        {
            foreach (var track in tracks) QueueManager.Default.AddToQueue(track.Uri);
            Writer.WriteSuccessLine("Tracks added to queue."); ;
        }
        else if(action == SelectedTracksAction.Playlist)
        {
            var user = UserManager.Default.GetCurrentUser();
            var playListName = DialogService.QuestionAnswerDialog("Name your new playlist");
            var description = DialogService.QuestionAnswerDialog("Describe your new playlist");
            var playlistId = PlaylistModifyManager.Default.CreatePlaylist(user.Id, playListName, $"{description}\nPlaylist created with SpotifyPrompt");
            PlaylistModifyManager.Default.AddTracksToPlaylist(playlistId, tracks.Select(t => t.Uri));
        }
        Writer.WriteTable(tracks.Select(t => new{Artist = t.Artists.FirstOrDefault()?.Name, Name = t.Name, Album = t.Album.Name, ReleaseDate = t.Album.ReleaseDate}));
    }
}