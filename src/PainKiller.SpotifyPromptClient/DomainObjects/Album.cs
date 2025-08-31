namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class Album : IContainsTags
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ArtistSimplified> Artists { get; set; } = [];
    public string ReleaseDate { get; set; } = string.Empty;
    public int TotalTracks { get; set; }
    public string Uri { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int ReleaseYear
    {
        get
        {
            var date = ReleaseDate;
            if (!string.IsNullOrEmpty(date))
            {
                var yearPart = date.Contains('-') ? date.Substring(0, date.IndexOf('-')) : date;
                if (int.TryParse(yearPart, out var year)) return year;
            }
            return 0;
        }
    }
    public override string ToString()
    {
        var artist = Artists?.FirstOrDefault()?.Name ?? "Unknown Artist";
        var tagsText = string.IsNullOrWhiteSpace(Tags) ? "" : $" [{Tags}]";
        return $"{artist} - {Name} ({ReleaseDate}){tagsText} ({TotalTracks} tracks)";
    }
}