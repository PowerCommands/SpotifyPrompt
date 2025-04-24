using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View queue", 
                      arguments: ["index"],
                       examples: ["//View Queue","queue"])]
public class QueueCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var queue = QueueManager.Default.GetQueue();
        var index = input.Arguments.Length > 0 ? int.Parse(input.Arguments[0]) : -1;
        
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
        Writer.WriteTable(queue.Select(t => new{Artist = t.Artists.First().Name,Title = t.Name ,Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4,"")}));
        return Ok();
    }
}