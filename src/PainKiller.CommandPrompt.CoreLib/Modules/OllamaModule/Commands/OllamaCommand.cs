using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Services;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;

namespace PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Commands;

[CommandDesign(
    description: "Manage and diagnose the Ollama server and its models.",
    options:
    [
        "status",
        "model",
        "installed",
        "remove",
        "download"
    ],
    examples:
    [
        "ollama --status",
        "ollama --details",
        "ollama --installed",
        "ollama --remove gemma3",
        "ollama --download DeepSeekCoder"
    ]
)]
public class OllamaCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var config = Configuration.Core.Modules.Ollama;
        var service = OllamaService.GetInstance(config.BaseAddress, config.Port, config.Model);
        
        if (!service.IsOllamaServerRunning())
        {
            Writer.WriteLine("Ollama server not running, attempting to start...");
            service.StartOllamaServer();
            Thread.Sleep(2000);

            if (!service.IsOllamaServerRunning())
            {
                Writer.WriteLine("Could not start Ollama-server, server not installed?\nGet it here:");
                Writer.WriteUrl("https://ollama.com/download");
                return Nok("Could not start Ollama-server, server not installed?");
            }
        }
        if (input.HasOption("status"))
        {
            Writer.WriteLine("Ollama server is running.");
            return Ok();
        }
        if (input.HasOption("model"))
        {
            service.ShowModelInfo(Configuration.Core.Modules.Ollama.Model);
            return Ok();
        }
        if (input.HasOption("installed"))
        {
            service.ShowInstalledModels();
            return Ok();
        }
        if (input.HasOption("remove"))
        {
            var modelName = input.GetOptionValue("remove");
            if (string.IsNullOrWhiteSpace(modelName))
            {
                Writer.WriteLine("Please specify the model to remove.");
                return Nok("Model name not provided.");
            }

            if (service.RemoveModel(modelName))
            {
                Writer.WriteLine($"Model '{modelName}' has been removed.");
                return Ok();
            }
            else
            {
                Writer.WriteLine($"Failed to remove model '{modelName}'.");
                return Nok("Removal failed.");
            }
        }
        if (input.HasOption("download"))
        {
            var modelName = input.GetOptionValue("download");
            if (string.IsNullOrWhiteSpace(modelName))
            {
                Writer.WriteLine("Please specify the model to download.");
                return Nok("Model name not provided.");
            }

            Writer.WriteLine($"Attempting to download model '{modelName}'...");
            if (service.DownloadModel(modelName))
            {
                Writer.WriteLine($"Model '{modelName}' downloaded successfully.");
                return Ok();
            }
            else
            {
                Writer.WriteLine($"Failed to download model '{modelName}'.");
                return Nok("Download failed.");
            }
        }
        return Ok("No action taken.");
    }
}