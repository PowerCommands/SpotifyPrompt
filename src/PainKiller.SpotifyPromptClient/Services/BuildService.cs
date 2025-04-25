using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Extensions;

namespace PainKiller.SpotifyPromptClient.Services;
public class BuildService : IBuildService
{
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
                    tracks = GetRandomTracks(template.Tags, template.YearSpan);
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

    private List<TrackObject> GetRandomTracks(List<string> tags, YearSpan years)
    {
        var tracks = StorageService<Tracks>.Service.GetObject().Items.Where(t => t.Tags.Split(',').Any(tg => tg.ToLower().Contains(string.Join(' ', tags).ToLower())) && years.IsInRange(t.ReleaseYear)).ToList();
        tracks.Shuffle();
        return tracks;
    }
}