namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IRandomRelatedArtistTrackGenerator
{
    List<TrackObject> GetRandomRelatedArtistsTracks(List<TrackObject> tracks, IAIManager aiManager, YearRange years, int count, int maxCountPerArtist);
}