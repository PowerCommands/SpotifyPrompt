using System.Text.Json.Serialization;

namespace PainKiller.SpotifyPromptClient.DomainObjects;

/// <summary>
/// Full artist object returned by GET /v1/artists/{id}
/// </summary>
public class Artist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("href")]
    public string Href { get; set; } = "";

    [JsonPropertyName("uri")]
    public string Uri { get; set; } = "";

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    [JsonPropertyName("popularity")]
    public int Popularity { get; set; }

    [JsonPropertyName("followers")]
    public Followers Followers { get; set; } = new Followers();
}