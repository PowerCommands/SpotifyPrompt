using System.Text.Json;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
namespace PainKiller.SpotifyPromptClient.Services;
public class WikipediaService : IWikipediaService
{
    private readonly ILogger<WikipediaService> _logger = LoggerProvider.CreateLogger<WikipediaService>();

    private WikipediaService() { }
    private static readonly Lazy<IWikipediaService> Instance = new(() => new WikipediaService());
    public static IWikipediaService Default => Instance.Value;
    public string TryFetchWikipediaIntro(string search)
    {
        var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(search)}";
        using var http = new HttpClient();
        var response = http.GetAsync(url).GetAwaiter().GetResult();
        _logger.LogInformation($"Response: {response.StatusCode}");
        if (!response.IsSuccessStatusCode) return "";
        
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("extract", out var ext) ? ext.GetString() ?? "" : "";
    }
}