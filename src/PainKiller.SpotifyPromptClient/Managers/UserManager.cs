using System.Net.Http.Headers;
using System.Text.Json;

namespace PainKiller.SpotifyPromptClient.Managers;

public class UserManager : SpotifyClientBase, IUserManager
{
    private UserManager() { }
    private static readonly Lazy<IUserManager> Instance = new(() => new UserManager());
    public static IUserManager Default => Instance.Value;
    public UserProfile GetCurrentUser()
    {
        var token = GetAccessToken();
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = _http.SendAsync(req).GetAwaiter().GetResult();
        resp.EnsureSuccessStatusCode();
        var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}