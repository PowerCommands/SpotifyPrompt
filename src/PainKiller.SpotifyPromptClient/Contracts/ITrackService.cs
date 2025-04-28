namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ITrackService
{
    /// <summary>
    /// Gets the currently playing track as a full TrackObject, or null if nothing is playing or it's not a track.
    /// </summary>
    TrackObject? GetCurrentlyPlayingTrack();

    /// <summary>
    /// Fetches a track by ID and maps it to TrackObject.
    /// </summary>
    TrackObject GetTrack(string trackId);

    /// <summary>
    /// Fetches tracks by Album ID and maps it to TrackObjects.
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    List<TrackObject> GetAlbumTracks(string albumId);
}