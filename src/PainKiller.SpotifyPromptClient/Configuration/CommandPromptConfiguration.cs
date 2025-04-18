using PainKiller.CommandPrompt.CoreLib.Configuration.DomainObjects;
namespace PainKiller.SpotifyPromptClient.Configuration;
public class CommandPromptConfiguration : ApplicationConfiguration
{
    public SpotifyConfiguration Spotify { get; set; } = new();
}