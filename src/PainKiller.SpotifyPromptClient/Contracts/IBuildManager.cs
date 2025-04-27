namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IBuildManager
{
    List<string> GetTags();
    string GetPlayListSummary(PlaylistTemplate template);
    List<TrackObject> GetPlaylist(PlaylistTemplate template, IAIManager aiManager);
}