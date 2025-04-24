namespace PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Configuration;

public class OllamaConfiguration
{
    public string BaseAddress { get; set; } = "localhost";
    public int Port { get; set; } = 11434;
    public string Model { get; set; } = "gemma3:latest";
}