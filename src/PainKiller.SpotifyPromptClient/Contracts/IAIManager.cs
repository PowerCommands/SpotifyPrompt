namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IAIManager
{
    List<string> GetSimilarArtists(string artistName);
    string GetCategory(string artistName);
    bool GetPredictionToQuery(string statement, string information, bool debugMode);
    string GetArtistAndSongTitle(string query);
    void ClearMessages();
}