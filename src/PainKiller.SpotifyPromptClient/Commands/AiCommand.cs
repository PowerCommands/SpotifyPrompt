using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

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
        var counter = 0;
        foreach (var artist in artists)
        {
            counter++;
            aiManager.ClearMessages();
            Writer.Clear();
            Writer.WriteDescription("AI", $"Enriching {artist.Name} with related artists ({counter} of {artists.Count})");
            if (relatedArtists.Any(a => a.Id == artist.Id) && keepExisting) continue;
            var relatedArtist = new RelatedArtist{Id = artist.Id, Name = artist.Name};
            var aiFoundArtists = aiManager.GetSimilarArtists(artist.Name);
            foreach (var aiFoundArtist in aiFoundArtists)
            {
                try
                {
                    if (string.IsNullOrEmpty(aiFoundArtist.Trim())) continue;
                    var foundArtist = SearchService.Default.SearchArtists(aiFoundArtist).FirstOrDefault(a => a.Name.Trim().ToLower() == aiFoundArtist.Trim().ToLower());
                    if (foundArtist == null) continue;
                    relatedArtist.Items.Add(foundArtist);
                    Writer.WriteLine($"Found related artist: {foundArtist.Name}");
                }
                catch (Exception e)
                {
                    Writer.WriteWarning($"{aiFoundArtist} not found on Spotify. {e.Message}", nameof(AiCommand));
                }
            }
            relatedArtistStorage.Insert(relatedArtist, a => a.Id == artist.Id);
        }
        return Ok();
    }
}