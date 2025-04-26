using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Extensions;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Services;
public class BuildService : IBuildService
{
    private readonly IConsoleWriter _writer = ConsoleService.Writer;
    private readonly ILogger<BuildService> _logger = LoggerProvider.CreateLogger<BuildService>();
    private BuildService() { }
    private static readonly Lazy<IBuildService> Instance = new(() => new BuildService());
    public static IBuildService Default => Instance.Value;
    public List<string> GetTags() => StorageService<Tracks>.Service.GetObject().Items.Select(t => t.Tags).Distinct().ToList();

    public string GetPlayListSummary(PlaylistTemplate template)
    {
        var count = -1;
        switch (template.SourceType)
        {
            case PlaylistSourceType.Tracks:
                if (template.RandomMode == RandomMode.Selected) count = SelectedService.Default.GetSelectedTracks().Count;
                break;
            case PlaylistSourceType.Albums:
                if (template.RandomMode == RandomMode.Selected) count = SelectedService.Default.GetSelectedAlbums().Count;
                break;
            case PlaylistSourceType.Artists:
                if (template.RandomMode == RandomMode.Selected) count = SelectedService.Default.GetSelectedArtists().Count;
                break;
            default:
                break;
        }
        var countLabel = count == -1 ? "∞" : count.ToString();
        return $"Type: {template.SourceType} Random mode:{template.RandomMode.ToString()} {countLabel}";
    }
    public List<TrackObject> GetPlaylist(PlaylistTemplate template, IAIManager aiManager)
    {
        var tracks = new List<TrackObject>();
        switch (template.SourceType)
        {
            case PlaylistSourceType.Tracks:
                if (template.RandomMode == RandomMode.Selected)
                {
                    tracks = SelectedService.Default.GetSelectedTracks();
                }
                else if (template.RandomMode == RandomMode.Related)
                {
                    var selectedTracks = GetRandomTracks(template.Tags, template.YearRange, template.MaxCountPerArtist);
                    tracks = GetRandomRelatedArtistsTracks(selectedTracks, aiManager, template.YearRange, template.Count, template.MaxCountPerArtist);
                }
                else
                {
                    tracks = GetRandomTracks(template.Tags, template.YearRange, template.MaxCountPerArtist);
                }
                break;
            case PlaylistSourceType.Albums:
                //tracks = GetAlbums(template);
                break;
            case PlaylistSourceType.Artists:
                //tracks = GetArtists(template);
                break;
            default:
                break;
        }
        return tracks.Take(template.Count).ToList();
    }
    private List<TrackObject> GetRandomTracks(List<string> tags, YearRange years, int maxCountPerArtist)
    {
        var tracks = StorageService<Tracks>.Service.GetObject().Items.Where(t => (t.Tags.Split(',').Any(tg => tg.ToLower().Contains(string.Join(' ', tags).ToLower())) || tags.First() == "*") && years.IsInRange(t.ReleaseYear)).ToList();
        tracks.Shuffle();
        if (maxCountPerArtist < 1) return tracks;
        var retVal = new List<TrackObject>();
        foreach (var track in tracks)
        {
            try
            {
                var count = retVal.Count(t => t.Artists.First().Id == track.Artists.First().Id);
                if(count >= maxCountPerArtist) continue;
                retVal.Add(track);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e?.Message, nameof(GetRandomTracks));
            }
        }
        return retVal;
    }
    private List<TrackObject> GetRandomRelatedArtistsTracks(List<TrackObject> tracks, IAIManager aiManager, YearRange years, int count, int maxCountPerArtist)
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
                var relatedTracks = GetRandomRelatedArtistsTracks(artist, aiManager, maxCountPerArtist);
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
        retVal.Shuffle();
        return retVal;
    }
    private List<TrackObject> GetRandomRelatedArtistsTracks(ArtistSimplified artist, IAIManager aiManager, int maxCountPerArtist)
    {
        var relatedArtists = aiManager.GetSimilarArtists(artist.Name).Where(a => !string.IsNullOrEmpty(a)).ToList();
        if (relatedArtists.Count == 0)
        {
            _logger.LogWarning($"No related artists found for {artist.Name}", nameof(GetRandomRelatedArtistsTracks));
            return new List<TrackObject>();
        }
        relatedArtists.Shuffle();
        var relatedArtist = relatedArtists.First();
        var query = $"artist:\"{relatedArtist}\"";
        _writer.WriteLine($"Searching for tracks by {relatedArtist}");
        var searchTracks = SearchManager.Default.SearchTracks(query);
        searchTracks.Shuffle();
        return searchTracks.Take(maxCountPerArtist).ToList();
    }
}