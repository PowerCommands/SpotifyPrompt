using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Services;

namespace PainKiller.SpotifyPromptClient.Managers;

public class AIManager(string baseAddress, int port, string model) : IAIManager
{
    private readonly ILogger<AIManager> _logger = LoggerProvider.CreateLogger<AIManager>();
    public List<string> GetSimilarArtists(string artistName)
    {
        var service = OllamaService.GetInstance(baseAddress, port, model);
        if (!service.IsOllamaServerRunning())
        {
            service.StartOllamaServer();
            Thread.Sleep(2000);

            if (!service.IsOllamaServerRunning())
            {
                _logger.LogError("Could not start Ollama service, you need to have Ollama installed, get it here: https://ollama.com/download");
            }
        }
        service.AddMessage(new ChatMessage("user", $"Generate a list of 10 artists that are similar to \"{artistName}\". Please provide only the artist names, one per line. Do not include any additional information, headlines or any other descriptions so that I can use the result programatically."));
        var response = service.SendChatToOllama().GetAwaiter().GetResult();
        var rows = response.Split('\n');
        return rows.Take(10).ToList();
    }
    public string GetCategory(string artistName)
    {
        var service = OllamaService.GetInstance(baseAddress, port, model);
        if (!service.IsOllamaServerRunning())
        {
            service.StartOllamaServer();
            Thread.Sleep(2000);

            if (!service.IsOllamaServerRunning())
            {
                _logger.LogError("Could not start Ollama service, you need to have Ollama installed, get it here: https://ollama.com/download");
            }
        }
        service.AddMessage(new ChatMessage("user", $"I want you to help me categorize artists, you must return ONE word only and that must be one of these \"Pop,Metal,Rock,HardRock,Punk,HipHop,RnB,Synth,Jazz,Blues,Country,Reggae,Classical\" for the artis \"{artistName}\" if you are unsure return \"Pop\""));
        var response = service.SendChatToOllama().GetAwaiter().GetResult();
        return $"{response}".Trim();
    }
    public void ClearMessages()
    {
        var service = OllamaService.GetInstance(baseAddress, port, model);
        service.ClearChatMessages();
    }
}