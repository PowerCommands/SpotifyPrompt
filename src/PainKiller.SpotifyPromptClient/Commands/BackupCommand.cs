using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Backup your persisted data", 
                       examples: ["//Backup","backup"])]
public class BackupCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var fName = StorageService<Albums>.Service.Backup();
        Writer.WriteSuccessLine($"{nameof(Albums)} backed up to {fName}");
        fName = StorageService<Artists>.Service.Backup();
        Writer.WriteSuccessLine($"{nameof(Artists)} backed up to {fName}");
        fName = StorageService<Playlists>.Service.Backup();
        Writer.WriteSuccessLine($"{nameof(Playlists)} backed up to {fName}");
        fName = StorageService<PlaylistTracks>.Service.Backup();
        Writer.WriteSuccessLine($"{nameof(PlaylistTracks)} backed up to {fName}");
        fName = StorageService<Tracks>.Service.Backup();
        Writer.WriteSuccessLine($"{nameof(Tracks)} backed up to {fName}");
        return Ok();
    }
}