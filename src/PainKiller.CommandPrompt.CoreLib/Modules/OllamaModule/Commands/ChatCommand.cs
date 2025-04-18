using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Commands;

public class ChatCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var config = Configuration.Core.Modules.Ollama;
        var service = OllamaService.GetInstance(config.BaseAddress, config.Port, config.Model);
        if (!service.IsOllamaServerRunning())
        {
            service.StartOllamaServer();
            Thread.Sleep(2000);

            if (!service.IsOllamaServerRunning())
            {
                Writer.WriteLine("Could not start Ollama-server, server not installed?\nGet it here:");
                Writer.WriteUrl("https://ollama.com/download");
                return Nok("Could not start Ollama-server, server not installed?");
            }
        }
        Writer.WriteLine($"Starts conversation with the model {Configuration.Core.Modules.Ollama.Model} (type /bye to end):");
        while (true)
        {
            Writer.Write("> ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput)) continue;
            if (userInput.Trim().Equals("/bye", StringComparison.OrdinalIgnoreCase)) break;

            service.AddMessage(new ChatMessage("user", userInput));
            var response = service.SendChatToOllama().GetAwaiter().GetResult();

            Writer.WriteLine(response);
            service.AddMessage(new ChatMessage("assistant", response));
        }
        return Ok();
    }
}