namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IQueueService
{
    List<TrackObject> GetQueue();
    void AddToQueue(string trackUri, string? deviceId = null);
}