using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View queue", 
                       examples: ["//View Queue","queue"])]
public class QueueCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var queue = QueueManager.Default.GetQueue();
        Writer.WriteTable(queue.Select(t => new{Artist = t.Artists.First().Name,Title = t.Name ,Album = t.Album.Name, Released = t.Album.ReleaseDate.Trim().Truncate(4," ")}));
        return Ok();
    }
}