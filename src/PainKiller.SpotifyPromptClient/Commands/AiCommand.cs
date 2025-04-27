using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Use AI to enrich your spotify data.",
                        options: ["keep-existing"],
                       examples: ["//Enrich artist with related artists", "ai"])]
public class AiCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var relatedArtistStorage = new ObjectStorage<RelatedArtists, RelatedArtist>();
        var artists = StorageService<Artists>.Service.GetObject().Items;
        var relatedArtists = relatedArtistStorage.GetItems();
        var keepExisting = input.HasOption("keep-existing");
        var config = Configuration.Core.Modules.Ollama;
        var aiManager = new AIManager(config.BaseAddress, config.Port, config.Model);
        foreach (var artist in artists)
        {
            if(relatedArtists.Any(a => a.Id == artist.Id) && keepExisting) continue;
            var related = new RelatedArtist{Id = artist.Id};
        }
        return Ok();
    }
}