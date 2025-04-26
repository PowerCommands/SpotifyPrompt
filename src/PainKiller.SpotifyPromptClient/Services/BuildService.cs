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
                else
                {
                    tracks = GetRandomTracks(template.Tags, template.YearRange, template.UniqueArtists);
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

    private List<TrackObject> GetRandomTracks(List<string> tags, YearRange years, bool unique)
    {
        var tracks = StorageService<Tracks>.Service.GetObject().Items.Where(t => t.Tags.Split(',').Any(tg => tg.ToLower().Contains(string.Join(' ', tags).ToLower())) && years.IsInRange(t.ReleaseYear)).ToList();
        tracks.Shuffle();
        if (!unique) return tracks;
        var retVal = new List<TrackObject>();
        foreach (var track in tracks)
        {
            try
            {
                var artist = track.Artists.First();
                if (retVal.All(t => t.Artists.First().Name != artist.Name)) retVal.Add(track);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e?.Message, nameof(GetRandomTracks));
            }
        }
        return retVal;
    }

    private List<TrackObject> GetRandomRelatedArtistsTracks(List<TrackObject> tracks, IAIManager aiManager, YearRange years, bool unique)
    {
        var unFiltered = new List<TrackObject>();
        foreach (var track in tracks)
        {
            try
            {
                var artist = track.Artists.First();
                var relatedTracks = GetRandomRelatedArtistsTracks(artist, aiManager);
                foreach (var relatedTrack in relatedTracks)
                {
                    if (unFiltered.All(t => t.Id != relatedTrack.Id)) unFiltered.Add(relatedTrack);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e?.Message, nameof(GetRandomRelatedArtistsTracks));
            }
        }
        unFiltered = unFiltered.Where(t => years.IsInRange(t.ReleaseYear)).ToList();
        var retVal = new List<TrackObject>();
        if (!unique) return retVal;
        foreach (var track in unFiltered)
        {
            try
            {
                var artist = track.Artists.First();
                if (retVal.All(t => t.Artists.First().Name != artist.Name)) retVal.Add(track);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e?.Message, nameof(GetRandomTracks));
            }
        }
        return retVal;
    }
    private List<TrackObject> GetRandomRelatedArtistsTracks(ArtistSimplified artist, IAIManager aiManager)
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
        return SearchManager.Default.SearchTracks(query);
    }
}