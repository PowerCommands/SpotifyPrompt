using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Artist command", 
                       examples: ["//Show your artists and their tracks","artist"])]
public class ArtistCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized() => ShellService.Default.Execute("spotify");
    public override RunResult Run(ICommandLineInput input)
    {
        var artistStorage = new SpotifyObjectStorage<Artists, ArtistSimplified>();
        var artists = artistStorage.GetItems();
        var selectedArtists = ListService.ShowSelectFromFilteredList<ArtistSimplified>("Select a playlist!", artists,(info, s) => info.Name.Contains(s,StringComparison.OrdinalIgnoreCase), Presentation, Writer);
        if (selectedArtists.Count == 0) return Ok();
        return Ok();
    }
    private void Presentation(List<ArtistSimplified> items) => Writer.WriteTable(items);
}