namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IPlaylistService
{
    void CreatePlaylist(string name, string description, List<TrackObject> tracks, List<string> tags, bool isPublic = false);
}