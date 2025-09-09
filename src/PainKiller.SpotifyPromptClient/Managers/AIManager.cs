using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Services;

namespace PainKiller.SpotifyPromptClient.Managers;

public class AIManager(string baseAddress, int port, string model) : IAIManager
{
    private readonly ILogger<AIManager> _logger = LoggerProvider.CreateLogger<AIManager>();

    private IOllamaService GetService()
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
        return service;
    }
    public List<string> GetSimilarArtists(string artistName)
    {
        var service = GetService();
        service.AddMessage(new ChatMessage("user", $"Generate a list of 10 artists that are similar to \"{artistName}\". Please provide only the artist names, one per line. Do not include any additional information, headlines or any other descriptions so that I can use the result programatically."));
        var response = service.SendChatToOllama().GetAwaiter().GetResult();
        var rows = response.Split('\n');
        return rows.Take(10).ToList();
    }
    public string GetCategory(string artistName)
    {
        var service = GetService();
        service.AddMessage(new ChatMessage("user", $"I want you to help me categorize artists, you must return ONE word only and that must be one of these \"Pop,Metal,Rock,HardRock,Punk,HipHop,RnB,Synth,Jazz,Blues,Country,Reggae,Classical\" for the artis \"{artistName}\" if you are unsure return \"Pop\""));
        var response = service.SendChatToOllama().GetAwaiter().GetResult();
        return $"{response}".Trim();
    }

    public string GetArtistAndSongTitle(string query)
    {
        var service = GetService();
        service.AddMessage(new ChatMessage("user", $"Return a suggestion for an artist's song based on the query that follows \"{query}\". Please provide only the artist name and the song title and nothing more."));
        return service.SendChatToOllama().GetAwaiter().GetResult();
    }
    public bool GetPredictionToQuery(string statement, string information, bool debugMode = false)
    {
        var service = GetService();
        var q1 = $"Here are some information about an artist or rockband I gonna make a statement about in my next input to you. {information}";
        service.AddMessage(new ChatMessage("user", q1));
        if(debugMode) ConsoleService.Writer.WriteDescription("AI", q1);
        var q2 = $"I want you answer true or false to my statement, you must answer true or false, nothing else, the statement is \"{statement}\" if you are unsure return false";
        if(debugMode) ConsoleService.Writer.WriteDescription("AI", q2);
        service.AddMessage(new ChatMessage("user", q2));
        var response = service.SendChatToOllama().GetAwaiter().GetResult();
        if(debugMode) ConsoleService.Writer.WriteDescription("AI", response);
        if(debugMode) Console.ReadLine();
        ConsoleService.Writer.Clear();
        ClearMessages();
        return $"{response}".Trim().ToLower() == "true";
    }
    public void ClearMessages()
    {
        var service = OllamaService.GetInstance(baseAddress, port, model);
        service.ClearChatMessages();
    }
}