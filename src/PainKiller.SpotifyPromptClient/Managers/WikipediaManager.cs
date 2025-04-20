using System.Text.Json;
namespace PainKiller.SpotifyPromptClient.Managers;
public class WikipediaManager : IWikipediaManager
{
    private WikipediaManager() { }
    private static readonly Lazy<IWikipediaManager> Instance = new(() => new WikipediaManager());
    public static IWikipediaManager Default => Instance.Value;
    public string TryFetchWikipediaIntro(string search)
    {
        var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(search)}";
        using var http = new HttpClient();
        var response = http.GetAsync(url).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode) return "";

        
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("extract", out var ext) ? ext.GetString() ?? "" : "";
    }
}