using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.Utils;

public static class GenreMapper
{
    private static readonly List<(Genres Genre, string[] Keys)> Mapping =
    [
        (Genres.HardRock, ["hard rock", "hardrock", "arena rock", "album rock", "glam rock", "modern rock", "garage rock", "soft rock", "surf rock"]),
        (Genres.Metal, ["metal", "death metal", "doom metal", "black metal", "power metal", "folk metal", "symphonic metal", "groove metal", "thrash metal", "speed metal", "nu metal", "melodic metal", "industrial metal"]),
        (Genres.Punk, ["punk", "hardcore punk", "emo", "post-punk", "skate punk", "anarcho punk", "proto-punk"]),
        (Genres.Rock, ["rock", "classic rock", "progressive rock", "stoner rock", "psychedelic rock", "space rock", "art rock", "post-grunge", "grunge"]),
        (Genres.HipHop, ["hip hop", "hip‑hop", "gangster rap", "trap", "boom bap", "rap", "cloud rap", "rage rap", "east coast hip hop", "west coast hip hop", "southern hip hop", "hardcore hip hop", "drill"]),
        (Genres.RnB, ["r&b", "rnb", "neo soul", "soul", "funk ", "disco"]),
        (Genres.Pop, ["pop", "poppunk", "synthpop", "electropop", "bubblegum pop", "hyperpop", "k-pop", "latin pop"]),
        (Genres.Synth, ["synth", "synthwave", "synthpop", "electro", "ebm", "electroclash"]),
        (Genres.Jazz, ["jazz", "jazz funk", "jazz fusion", "vocal jazz", "acid jazz"]),
        (Genres.Blues, ["blues", "blues rock", "modern blues"]),
        (Genres.Country, ["country", "country rock", "pop country", "red dirt", "texas country"]),
        (Genres.Reggae, ["reggae", "dancehall", "dub", "rocksteady", "roots reggae"]),
        (Genres.Classical, ["classical", "orchestra", "chamber", "choral", "piano", "opera"])
    ];
    public static Genres Map(string rawGenre)
    {
        var text = rawGenre.ToLowerInvariant();
        foreach (var (genre, keys) in Mapping)
        {
            if (keys.Any(k => text.Contains(k)))
                return genre;
        }
        return Genres.Unknown;
    }
    public static void CopyGenres()
    {
        var simpleArtists = StorageService<Artists>.Service.GetObject().Items;
        var artists = ArtistManager.Default.GetArtists(simpleArtists.Select(a => a.Id));

        var genres = new List<string>();
        foreach (var artist in artists)
        {
            if(artist == null || artist.Genres == null || artist.Genres.Count == 0)
                continue;
            foreach (var genre in artist.Genres.Where(genre => genres.All(g => g != genre)))
            {
                genres.Add(genre);
            }
        }
        var genreString = string.Join('\n', genres);
        TextCopy.ClipboardService.SetText(genreString);
        ConsoleService.Writer.WriteSuccessLine("Genres copied to clipboard.");
    }
}