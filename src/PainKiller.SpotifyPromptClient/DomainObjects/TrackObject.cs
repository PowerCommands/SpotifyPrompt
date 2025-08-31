namespace PainKiller.SpotifyPromptClient.DomainObjects;

public class TrackObject : IContainsTags
{
    public TrackObject(){}

    public TrackObject(TrackSimplified trackSimplified)
    {
        Id = trackSimplified.Id;
        Name = trackSimplified.Name;
        Uri = trackSimplified.Uri;
        DurationMs = trackSimplified.DurationMs;
    }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ArtistSimplified> Artists { get; set; } = [];
    public Album Album { get; set; } = new();
    public int DurationMs { get; set; }
    public string Uri { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int ReleaseYear
    {
        get
        {
            var date = Album.ReleaseDate;
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
        return $"{artist} - {Name} ({ReleaseYear})";
    }
}