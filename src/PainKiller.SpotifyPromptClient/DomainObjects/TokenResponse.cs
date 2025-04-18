using System.Text.Json.Serialization;

namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public TimeSpan TimeUntilExpiration 
        => RetrievedAt.AddSeconds(ExpiresIn) - DateTime.UtcNow;

    [JsonIgnore]
    public bool IsExpired 
        => TimeUntilExpiration <= TimeSpan.Zero;
}