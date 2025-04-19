namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IPlayerManager
{
    void Play();
    void Pause();
    void Next();
    void Previous();
    (string? TrackName, string? Artists) GetCurrentlyPlaying();
}