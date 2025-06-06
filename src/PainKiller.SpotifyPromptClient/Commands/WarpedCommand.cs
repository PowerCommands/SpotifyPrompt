using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Warped, get statistic top tracks or top artists.",
                        options: ["artists","limit"],
                    suggestions: ["long_term","medium_term","short_term"],
                       examples: ["//Show top tracks (default) medium term (default","warped","//Show top artists long term","warped long_term --artists"])]
public class WarpedCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var timeRange = this.GetSuggestion(input.Arguments.FirstOrDefault(), "medium_term");
        input.TryGetOption(out var limit, 30);
        if (input.HasOption("artists"))
        {
            Writer.WriteHeadLine("Top artists");
            var artists = UserService.Default.GetTopArtists(limit, timeRange);
            SelectedManager.Default.UpdateSelected(artists);
            Writer.WriteTable(artists.Select(a => new{Name = a.Name, Tags = a.Tags}));
            return Ok();
        }
        Writer.WriteHeadLine("Top tracks");
        var tracks = UserService.Default.GetTopTracks(limit, timeRange);
        SelectedManager.Default.UpdateSelected(tracks);
        Writer.WriteTable(tracks.Select(t => new { Artist = t.Artists?.First().Name , Name = t.Name }));
        return Ok();
    }
}