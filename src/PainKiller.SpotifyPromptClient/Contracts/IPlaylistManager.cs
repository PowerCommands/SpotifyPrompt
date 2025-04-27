namespace PainKiller.SpotifyPromptClient.Contracts;
public interface IPlaylistManager
{
    void CreatePlaylist(string name, string description, List<TrackObject> tracks, List<string> tags, bool isPublic = false);
    void DeletePlaylist(string id);
}