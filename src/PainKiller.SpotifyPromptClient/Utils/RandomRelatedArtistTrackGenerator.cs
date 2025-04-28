using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Extensions;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Utils;
public class RandomRelatedArtistTrackGenerator(IAIManager aiManager) : IRandomRelatedArtistTrackGenerator
{
    private readonly IConsoleWriter _writer = ConsoleService.Writer;
    private readonly ILogger<RandomRelatedArtistTrackGenerator> _logger = LoggerProvider.CreateLogger<RandomRelatedArtistTrackGenerator>();
    public List<TrackObject> GetRandomRelatedArtistsTracks(List<TrackObject> tracks, IAIManager aiManager, YearRange years, int count, int maxCountPerArtist)
    {
        var retVal = new List<TrackObject>();
        var usedArtist = new List<ArtistSimplified>();
        foreach (var track in tracks)
        {
            try
            {
                aiManager.ClearMessages();
                var artist = track.Artists.First();
                if(usedArtist.Any(a => a.Id == artist.Id)) continue;
                usedArtist.Add(artist);
                var relatedTracks = GetRandomRelatedArtistsTracks(artist, aiManager, maxCountPerArtist, years);
                foreach (var relatedTrack in relatedTracks)
                {
                    if (retVal.All(t => t.Id != relatedTrack.Id))
                    {
                        if(retVal.Any(u => u.Id == relatedTrack.Id)) continue;
                        if(!years.IsInRange(relatedTrack.ReleaseYear)) continue;
                        
                        _writer.WriteLine($"Track '{relatedTrack.Name}' added");
                        retVal.Add(relatedTrack);
                    }
                    if (retVal.Count >= count) break;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e?.Message, nameof(GetRandomRelatedArtistsTracks));
            }
            if (retVal.Count >= count) break;
        }
        if (retVal.Count < count)
        {
            retVal.AddRange(GetRandomRelatedArtistsTracks(tracks, aiManager, years, count - retVal.Count, maxCountPerArtist));
        }
        retVal.Shuffle();
        return retVal;
    }
    private List<TrackObject> GetRandomRelatedArtistsTracks(ArtistSimplified artist, IAIManager aiManager, int maxCountPerArtist, YearRange years)
    {
        var relatedArtistStorage = new ObjectStorage<RelatedArtists, RelatedArtist>();
        var storedArtists = relatedArtistStorage.GetItems();
        var storedArtist = storedArtists.FirstOrDefault(a => a.Id == artist.Id);
        List<string> relatedArtists;
        if(storedArtist != null && storedArtist.Items.Count > 0) relatedArtists = storedArtist.Items.Select(a => a.Name).Take(maxCountPerArtist).ToList();
        else relatedArtists = aiManager.GetSimilarArtists(artist.Name).Where(a => !string.IsNullOrEmpty(a)).ToList();
        if (relatedArtists.Count == 0)
        {
            _logger.LogWarning($"No related artists found for {artist.Name}", nameof(GetRandomRelatedArtistsTracks));
            return new List<TrackObject>();
        }
        relatedArtists.Shuffle();
        var relatedArtist = relatedArtists.First();
        var query = $" artist:{relatedArtist} year:{years}";
        _writer.WriteLine($"Searching for tracks by {relatedArtist}");
        var searchTracks = SearchService.Default.SearchTracks(query);
        searchTracks.Shuffle();
        return searchTracks.Take(maxCountPerArtist).ToList();
    }
}