using System.Text.Json.Serialization;
namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class UserProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "";

    [JsonPropertyName("product")]
    public string Product { get; set; } = "";

    [JsonPropertyName("followers")]
    public Followers Followers { get; set; } = new Followers();
}