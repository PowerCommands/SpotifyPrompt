namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IBuildService
{
    List<string> GetTags();
    string GetPlayListSummary(PlaylistTemplate template);
    List<TrackObject> GetPlaylist(PlaylistTemplate template, IAIManager aiManager);
}