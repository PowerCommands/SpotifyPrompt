using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View queue", 
                      arguments: ["index","selected"],
                    suggestions: ["1","selected"],
                       examples: ["//View Queue","queue"])]
public class QueueCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var queue = QueueManager.Default.GetQueue();
        int.TryParse(input.Arguments[0], out var index);
        var selected = this.GetSuggestion(input.Arguments.FirstOrDefault(), "");
        if (index > 0)
        {
            var selectedTracks = SelectedService.Default.GetSelectedTracks();
            if (index < selectedTracks.Count)
            {
                var selectedTrack = selectedTracks[index];
                QueueManager.Default.AddToQueue(selectedTrack.Uri);
                Writer.WriteSuccessLine($"{selectedTrack.Name} added to queue");
                return Ok();
            }
        }
        else if (selected == "selected")
        {
            foreach (var trackObject in queue) QueueManager.Default.AddToQueue(trackObject.Uri);
        }
        Writer.WriteTable(queue.Select(t => new { Artist = t.Artists.First().Name, Title = t.Name, Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4, "") }));
        return Ok();
    }
}