using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View queued tracks", 
                      arguments: ["index","selected"],
                    suggestions: ["1","selected"],
                       examples: ["//View Queue","queue"])]
public class QueueCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var queue = QueueService.Default.GetQueue();
        int.TryParse(input.Arguments.FirstOrDefault(), out var index);
        var selected = this.GetSuggestion(input.Arguments.FirstOrDefault(), "");
        if (index > 0)
        {
            var selectedTracks = SelectedManager.Default.GetSelectedTracks();
            if (index < selectedTracks.Count)
            {
                var selectedTrack = selectedTracks[index];
                QueueService.Default.AddToQueue(selectedTrack.Uri);
                Writer.WriteSuccessLine($"{selectedTrack.Name} added to queue");
                return Ok();
            }
        }
        else if (selected == "selected")
        {
            foreach (var trackObject in queue) QueueService.Default.AddToQueue(trackObject.Uri);
        }
        SelectedManager.Default.UpdateSelected(queue);
        Writer.WriteTable(queue.Select(t => new { Artist = t.Artists.First().Name, Title = t.Name, Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4, "") }));
        return Ok();
    }
}