namespace PainKiller.SpotifyPromptClient.Contracts;

public interface IRandomRelatedArtistTrackGenerator
{
    List<TrackObject> GetRandomRelatedArtistsTracks(List<TrackObject> tracks, YearRange years, int count, int maxCountPerArtist);
}