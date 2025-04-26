using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Extensions;

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
                if(template.RandomMode == RandomMode.Selected) count = SelectedService.Default.GetSelectedTracks().Count;
                break;
            case PlaylistSourceType.Albums:
                if(template.RandomMode == RandomMode.Selected) count = SelectedService.Default.GetSelectedAlbums().Count;
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
    public List<TrackObject> GetPlaylist(PlaylistTemplate template)
    {
        var tracks = new List<TrackObject>();
        switch (template.SourceType)
        {
            case PlaylistSourceType.Tracks:
                if(template.RandomMode == RandomMode.Selected)
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
        if(!unique) return tracks;
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
}