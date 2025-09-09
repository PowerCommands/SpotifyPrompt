using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Ask AI to play a song.",
                        options: ["count","includeRelated"],
                       examples: ["//Ask AI to play a song", "Play a song with Red hot chili peppers"])]
public class AskCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var question = input.GetSearchString();
        var config = Configuration.Core.Modules.Ollama;
        var aiManager = new AIManager(config.BaseAddress, config.Port, config.Model);
        Writer.WriteHeadLine($"Find a song based on your question \"{question}\"\nusing {config.Model} on {config.BaseAddress}:{config.Port} please wait...");
        aiManager.ClearMessages();
        var aiSuggestion = aiManager.GetArtistAndSongTitle(question);
        var tracks = SearchService.Default.SearchTracks(aiSuggestion);
        IPlayerService playerManager = new PlayerService();
        playerManager.Play(tracks.Select(t => t.Uri).Take(1));
        InfoPanelService.Instance.Update();
        return Ok();
    }
}