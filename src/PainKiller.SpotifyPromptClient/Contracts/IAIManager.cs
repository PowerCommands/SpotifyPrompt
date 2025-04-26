namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IAIManager
{
    List<string> GetSimilarArtists(string artistName);
    string GetCategory(string artistName);
    void ClearMessages();
}