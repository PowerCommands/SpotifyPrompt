using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Managers;
public class PlaylistManager : IPlaylistManager
{
    private PlaylistManager() { }
    private static readonly Lazy<IPlaylistManager> Instance = new(() => new PlaylistManager());
    public static IPlaylistManager Default => Instance.Value;
    public void CreatePlaylist(string name, string description, List<TrackObject> tracks, List<string> tags, string prefix = "SPC", bool isPublic = false)
    {
        var playlistStorage = new ObjectStorage<Playlists, PlaylistInfo>();
        var playlistTrackStorage = new ObjectStorage<PlaylistTracks, PlaylistWithTracks>();

        var prefixAttribute = string.IsNullOrEmpty(prefix) ? "" : $"{prefix} - ";

        var id = PlaylistModifyManager.Default.CreatePlaylist(UserService.Default.GetCurrentUser().Id, $"`{prefixAttribute}{name}", description, isPublic);
        PlaylistModifyManager.Default.AddTracksToPlaylist(id, tracks.Select(t => t.Uri).ToList());
        playlistStorage.Insert(new PlaylistInfo { Id = id, Name = name, Owner = UserService.Default.GetCurrentUser().Id, Tags = string.Join(',', tags), TrackCount = tracks.Count }, playlist => playlist.Id == id);
        playlistTrackStorage.Insert(new PlaylistWithTracks { Id = id, Items = tracks }, playlist => playlist.Id == id);
    }
    public void DeletePlaylist(string id)
    {
        var playlistStorage = new ObjectStorage<Playlists, PlaylistInfo>();
        var playlistTrackStorage = new ObjectStorage<PlaylistTracks, PlaylistWithTracks>();
        PlaylistModifyManager.Default.DeletePlaylist(id);
        playlistTrackStorage.Remove(pts => pts.Id == id);
        playlistStorage.Remove(playlist => playlist.Id == id);
    }
}