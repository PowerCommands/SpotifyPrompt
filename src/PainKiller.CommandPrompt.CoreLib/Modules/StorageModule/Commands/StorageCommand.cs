using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Enums;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Commands;

[CommandDesign(description:"Command displays your stored data files",
                  examples: ["//Show all storded data files","storage"])]
public class StorageCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        var dir = StorageService<Description>.Service.GetRootDirectory();
        var backupDir = StorageService<Description>.Service.GetBackupDirectory();
        if (dir.Exists && backupDir.Exists) return;
        InitDirectories(dir.FullName, backupDir.FullName);
    }
    private void InitDirectories(string rootDir, string backupDir)
    {
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(backupDir);
    }

    public override RunResult Run(ICommandLineInput input)
    {
        var dir = StorageService<Description>.Service.GetRootDirectory();
        Writer.WriteHeadLine($"{Emo.Directory.Icon()} App directory {dir.FullName} {dir.GetDirectorySize().GetDisplayFormattedFileSize()}");
        foreach (var file in dir.GetFiles()) Writer.WriteLine($"├──{Emo.File.Icon()} {file.Name}");
        Environment.CurrentDirectory = dir.FullName;
        EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(Environment.CurrentDirectory));
        return Ok();
    }

    private class Description { private string Name { get; set; } = "Sven Gurra Aktersnurra"; }
}