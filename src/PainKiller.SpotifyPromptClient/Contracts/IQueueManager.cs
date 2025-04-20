namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IQueueManager
{
    List<TrackObject> GetQueue();
    void AddToQueue(string trackUri, string? deviceId = null);
}