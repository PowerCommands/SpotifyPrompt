using System.Text.Json.Serialization;

namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class Followers
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}