using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Extensions;
using PainKiller.SpotifyPromptClient.Services;
using PainKiller.SpotifyPromptClient.Utils;

namespace PainKiller.SpotifyPromptClient.Managers;
public class BuildManager : IBuildManager
{
    private readonly ILogger<BuildManager> _logger = LoggerProvider.CreateLogger<BuildManager>();
    private BuildManager() { }
    private static readonly Lazy<IBuildManager> Instance = new(() => new BuildManager());
    public static IBuildManager Default => Instance.Value;
    public List<string> GetTags()
    {
        var retVal = new List<string>();
        foreach (var tags in StorageService<Tracks>.Service.GetObject().Items.Select(t => t.Tags).Distinct().ToList())
        {
            var tagList = tags.Split(',').ToList();
            foreach (var tag in tagList)
            {
                if (retVal.All(t => t.ToLower() != tag.ToLower()))
                {
                    retVal.Add(tag);
                }
            }
        }
        retVal.Sort();
        return retVal;
    }
    public string GetPlayListSummary(PlaylistTemplate template)
    {
        var count = -1;
        switch (template.SourceType)
        {
            case PlaylistSourceType.Tracks:
                if (template.RandomMode == RandomMode.Selected) count = SelectedManager.Default.GetSelectedTracks().Count;
                break;
            case PlaylistSourceType.Albums:
                if (template.RandomMode == RandomMode.Selected) count = template.Ids.Count == 0 ? SelectedManager.Default.GetSelectedAlbums().Count : template.Ids.Count;
                break;
            case PlaylistSourceType.Artists:
                if (template.RandomMode == RandomMode.Selected) count = template.Ids.Count == 0 ? SelectedManager.Default.GetSelectedAlbums().Count : template.Ids.Count;
                break;
        }
        var countLabel = count == -1 ? "∞" : count.ToString();
        return $"Type: {template.SourceType} Random mode:{template.RandomMode.ToString()} {countLabel}";
    }
    public List<TrackObject> GetPlaylist(PlaylistTemplate template, IAIManager aiManager)
    {
        var randomRelatedArtistTracks = new RandomRelatedArtistTrackGenerator(aiManager);
        var tracks = new List<TrackObject>();
        switch (template.SourceType)
        {
            case PlaylistSourceType.Tracks:
                if (template.RandomMode == RandomMode.Selected)
                {
                    tracks = SelectedManager.Default.GetSelectedTracks();
                    tracks.Shuffle();
                }
                else if (template.RandomMode == RandomMode.Related)
                {
                    var selectedTracks = GetRandomTracks(template.Tags, template.YearRange, template.MaxCountPerArtist);
                    tracks = randomRelatedArtistTracks.GetRandomRelatedArtistsTracks(selectedTracks, template.YearRange, template.Count, template.MaxCountPerArtist);
                }
                else
                {
                    tracks = GetRandomTracks(template.Tags, template.YearRange, template.MaxCountPerArtist);
                }
                break;
            case PlaylistSourceType.Albums:
                if (template.RandomMode == RandomMode.Selected)
                {
                    tracks = GetRandomTracksByAlbum(SelectedManager.Default.GetSelectedAlbums().Select(a => a.Id).ToList(), template.MaxCountPerArtist);
                    tracks.Shuffle();
                }
                else if (template.RandomMode == RandomMode.Related)
                {
                    var selectedAlbums = GetRandomAlbums(template.Tags, template.YearRange);
                    var selectedTracks = GetRandomTracksByAlbum(selectedAlbums.Select(a => a.Id).ToList(), template.MaxCountPerArtist);
                    tracks = randomRelatedArtistTracks.GetRandomRelatedArtistsTracks(selectedTracks, template.YearRange, template.Count, template.MaxCountPerArtist);
                }
                else
                {
                    var selectedAlbums = GetRandomAlbums(template.Tags, template.YearRange);
                    tracks = GetRandomTracksByAlbum(selectedAlbums.Select(a => a.Id).ToList(), template.MaxCountPerArtist);
                }
                break;
            case PlaylistSourceType.Artists:
                if (template.RandomMode == RandomMode.Selected)
                {
                    tracks = GetRandomTracks(SelectedManager.Default.GetSelectedArtists(), template.MaxCountPerArtist);
                    tracks.Shuffle();
                }
                else if (template.RandomMode == RandomMode.Related)
                {
                    var selectedArtists = GetRandomArtists(template.Tags);
                    var selectedTracks = GetRandomTracks(selectedArtists, template.MaxCountPerArtist);
                    tracks = randomRelatedArtistTracks.GetRandomRelatedArtistsTracks(selectedTracks, template.YearRange, template.Count, template.MaxCountPerArtist);
                }
                else
                {
                    var selectedArtists = GetRandomAlbums(template.Tags, template.YearRange);
                    tracks = GetRandomTracksByAlbum(selectedArtists.Select(a => a.Id).ToList(), template.MaxCountPerArtist);
                }
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
                _logger.LogWarning(e?.Message, nameof(GetRandomTracksByAlbum));
            }
        }
        return retVal;
    }
    private List<TrackObject> GetRandomTracksByAlbum(List<string> albumIds, int maxCountPerArtist)
    {
        var retVal = new List<TrackObject>();
        foreach (var albumId in albumIds)
        {
            var tracks = TrackService.Default.GetAlbumTracks(albumId);
            foreach (var track in tracks)
            {
                try
                {
                    var count = retVal.Count(t => t.Artists.First().Id == track.Artists.First().Id);
                    if (count >= maxCountPerArtist) continue;
                    retVal.Add(track);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e?.Message, nameof(GetRandomTracksByAlbum));
                }
            }
        }
        retVal.Shuffle();
        return retVal;
    }
    private List<Album> GetRandomAlbums(List<string> tags, YearRange years)
    {
        var matchedAlbums = StorageService<Albums>.Service.GetObject().Items.Where(t => (t.Tags.Split(',').Any(tg => tg.ToLower().Contains(string.Join(' ', tags).ToLower())) || tags.First() == "*") && years.IsInRange(t.ReleaseYear)).ToList();
        matchedAlbums.Shuffle();
        return matchedAlbums;
    }
    private List<TrackObject> GetRandomTracks(List<ArtistSimplified> artists, int maxCountPerArtist)
    {
        var retVal = new List<TrackObject>();
        foreach (var artist in artists)
        {
            var tracks = ArtistService.Default.GetTopTracks(artist.Id, UserService.Default.GetCurrentUser().Country);
            var searchTrack = SearchService.Default.SearchTracks($"artist:{artist.Name}");
            tracks.AddRange(searchTrack);
            tracks =  tracks.DistinctBy(o => o.Id).ToList();
            foreach (var track in tracks)
            {
                try
                {
                    var count = retVal.Count(t => t.Artists.First().Id == track.Artists.First().Id);
                    if (count >= maxCountPerArtist) continue;
                    retVal.Add(track);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e?.Message, nameof(GetRandomTracksByAlbum));
                }
            }
        }
        retVal.Shuffle();
        return retVal;
    }
    private List<ArtistSimplified> GetRandomArtists(List<string> tags)
    {
        var matchedArtists = StorageService<Artists>.Service.GetObject().Items.Where(t => (t.Tags.Split(',').Any(tg => tg.ToLower().Contains(string.Join(' ', tags).ToLower())) || tags.First() == "*")).ToList();
        matchedArtists.Shuffle();
        return matchedArtists;
    }
}