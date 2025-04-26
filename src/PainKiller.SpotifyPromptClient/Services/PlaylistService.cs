using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Services;
public class PlaylistService : IPlaylistService
{
    private PlaylistService() { }
    private static readonly Lazy<IPlaylistService> Instance = new(() => new PlaylistService());
    public static IPlaylistService Default => Instance.Value;
    public void CreatePlaylist(string name, string description, List<TrackObject> tracks, List<string> tags,bool isPublic = false)
    {
        var playlistStorage = new ObjectStorage<Playlists, PlaylistInfo>();
        var playlistTrackStorage = new ObjectStorage<PlaylistTracks, PlaylistWithTracks>();

        var id = PlaylistModifyManager.Default.CreatePlaylist(UserManager.Default.GetCurrentUser().Id, name, description, isPublic);
        PlaylistModifyManager.Default.AddTracksToPlaylist(id, tracks.Select(t => t.Uri).ToList());
        playlistStorage.Insert(new PlaylistInfo { Id = id, Name = name, Owner = UserManager.Default.GetCurrentUser().Id, Tags = string.Join(',', tags), TrackCount = tracks.Count}, playlist => playlist.Id == id);
        playlistTrackStorage.Insert(new PlaylistWithTracks { Id = id, Items = tracks }, playlist => playlist.Id == id);
    }
}