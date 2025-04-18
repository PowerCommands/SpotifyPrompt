using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PainKiller.SpotifyPromptClient.DomainObjects;

namespace PainKiller.SpotifyPromptClient.Managers;
public class AuthorizationCodeFlowManager(string clientId, string redirectUri, string[] scopes)
{
    private readonly string _redirectUri = redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/";
    private string _codeVerifier = string.Empty;
    public async Task<string> AuthenticateAsync()
    {
        _codeVerifier  = GenerateCodeVerifier();
        var challenge  = CreateCodeChallenge(_codeVerifier);
        var scopeParam = Uri.EscapeDataString(string.Join(" ", scopes));

        using var listener = new HttpListener();
        listener.Prefixes.Add(_redirectUri);  // must end with slash
        listener.Start();

        var authUrl =
            $"https://accounts.spotify.com/authorize" +
            $"?client_id={clientId}" +
            $"&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
            $"&code_challenge_method=S256" +
            $"&code_challenge={challenge}" +
            $"&scope={scopeParam}";

        Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

        var context = await listener.GetContextAsync();
        var code    = context.Request.QueryString["code"];

        var html = Encoding.UTF8.GetBytes("<html><body>OK — you can close this window.</body></html>");
        context.Response.ContentLength64 = html.Length;
        await context.Response.OutputStream.WriteAsync(html, 0, html.Length);
        listener.Stop();
        return code!;
    }
    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        using var http = new HttpClient();
        var payload = new Dictionary<string, string>
        {
            ["grant_type"]   = "authorization_code",
            ["code"]         = code,
            ["redirect_uri"] = _redirectUri,
            ["client_id"]    = clientId,
            ["code_verifier"]= _codeVerifier
        };

        var res = await http.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(payload));
        res.EnsureSuccessStatusCode();

        var json  = await res.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        token.RetrievedAt = DateTime.UtcNow;
        return token;
    }
    public async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        using var http = new HttpClient();
        var payload = new Dictionary<string, string>
        {
            ["grant_type"]    = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"]     = clientId
        };

        var res = await http.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(payload));
        res.EnsureSuccessStatusCode();

        var json  = await res.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        token.RefreshToken = token.RefreshToken ?? refreshToken;
        token.RetrievedAt = DateTime.UtcNow;
        return token;
    }
    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
    private static string CreateCodeChallenge(string verifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}