using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Warped, get statistic top tracks or top artists",
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
            var artists = UserManager.Default.GetTopArtists(limit, timeRange);
            Writer.WriteTable(artists.Select(a => new{Name = a.Name, Tags = a.Tags}));
            return Ok();
        }
        Writer.WriteHeadLine("Top tracks");
        var tracks = UserManager.Default.GetTopTracks(limit, timeRange);
        Writer.WriteTable(tracks.Select(t => new { Artist = t.Artists?.First().Name , Name = t.Name }));
        return Ok();
    }
}