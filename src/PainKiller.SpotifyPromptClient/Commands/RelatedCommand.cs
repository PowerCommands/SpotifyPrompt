using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Use AI to enrich your spotify queue.",
                        options: ["count","includeRelated"],
                       examples: ["//Use AI to enrich your spotify queue", "related"])]
public class RelatedCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        input.TryGetOption(out var count, 10);
        input.TryGetOption(out var includeRelated, false);

        if (string.IsNullOrEmpty(filter))
        {
            IPlayerService playerManager = new PlayerService();
            var currentTrackPlaying = playerManager.GetCurrentTrack();
            if (currentTrackPlaying != null) filter = currentTrackPlaying.Artists.First().Name;
        }
        var config = Configuration.Core.Modules.Ollama;
        var aiManager = new AIManager(config.BaseAddress, config.Port, config.Model);
        Writer.WriteHeadLine($"Finding related artists to {filter} using {config.Model} on {config.BaseAddress}:{config.Port} please wait...");
        aiManager.ClearMessages();
        var aiFoundArtists = aiManager.GetSimilarArtists(filter).Take(count).ToList();
        if(includeRelated) aiFoundArtists.Add(filter);
        var tracks = new List<TrackObject>();
        foreach (var aiFoundArtist in aiFoundArtists)
        {
            try
            {
                if (string.IsNullOrEmpty(aiFoundArtist.Trim())) continue;
                var foundArtist = SearchService.Default.SearchArtists(aiFoundArtist);
                if (foundArtist == null) continue;
                var topTracks = SearchService.Default.SearchTracks($"artist:{foundArtist.First().Name}");
                var random = new Random();
                var index = random.Next(topTracks.Count);
                var randomTrack = topTracks[index];
                tracks.Add(randomTrack);
            }
            catch (Exception e)
            {
                Writer.WriteWarning($"{aiFoundArtist} not found on Spotify. {e.Message}", nameof(RelatedCommand));
            }
        }
        foreach (var track in tracks) QueueService.Default.AddToQueue(track.Uri);
        var queueTracks = QueueService.Default.GetQueue();
        Writer.WriteHeadLine("Related artists track added to queue");
        Writer.WriteHeadLine("----------------------------------------------------------------------------------------");
        Writer.WriteTable(queueTracks.Select(t => new { Artist = t.Artists.First().Name, t.Name, t.Album}));
        
        var confirmNewPlayList = DialogService.YesNoDialog("Do you want to create a new playlist with these tracks?");
        if (!confirmNewPlayList) return Ok();
        var name = DialogService.QuestionAnswerDialog("Name of the playlist:");
        PlaylistManager.Default.CreatePlaylist(name, $"Releated artist to {filter} created by {Configuration.Core.Name}", tracks, ["SPC"]);
        return Ok();
    }
}