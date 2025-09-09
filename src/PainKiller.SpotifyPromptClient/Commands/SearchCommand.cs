using PainKiller.SpotifyPromptClient.Managers;
using System.Text;
using PainKiller.SpotifyPromptClient.Services;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Search artist, tracks and albums.",
                        options: ["years", "genre", "artist", "limit", "tag:hipster", "tag:new", "upc", "isrc"],
                    suggestions: ["playlist", "track", "artist", "album"],
                       examples: ["//Search tracks", "search tracks \"Balls to the wall\""])]
public class SearchCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        input.TryGetOption(out int limit, 50);
        input.TryGetOption(out string years, string.Empty);
        input.TryGetOption(out string genre, string.Empty);
        input.TryGetOption(out string upc, string.Empty);
        input.TryGetOption(out string isrc, string.Empty);
        input.TryGetOption(out string artist, string.Empty);

        var filters = new List<string>();
        if (input.HasOption("year")) filters.Add($"year:{years}");
        if (!string.IsNullOrEmpty(genre) && input.HasOption("genre")) filters.Add($"genre:{genre}");
        if (!string.IsNullOrEmpty(upc) && input.HasOption("upc")) filters.Add($"upc:{upc}");
        if (!string.IsNullOrEmpty(isrc) && input.HasOption("isrc")) filters.Add($"isrc:{isrc}");
        if (input.HasOption("tag:hipster")) filters.Add("tag:hipster");
        if (input.HasOption("tag:new")) filters.Add("tag:new");

        var searchTerm = input.GetSearchString();
        if (string.IsNullOrEmpty(searchTerm))
        {
            Writer.WriteLine("No search term provided.", scope: nameof(SearchCommand));
            return Ok();
        }
        var searchType = this.GetSuggestion(input.Arguments.FirstOrDefault(), "track");
        var queryBuilder = new StringBuilder(searchTerm);
        foreach (var f in filters) queryBuilder.Append(' ').Append(f);
        var query = queryBuilder.ToString().Replace("playlist","").Replace("track","").Replace("artist","").Replace("album","");
        if (!string.IsNullOrEmpty(artist))
        {
            queryBuilder.Append($" artist:{artist}");
            query = queryBuilder.ToString().Replace("playlist","").Replace("track","").Replace("album","");
        }
        if (string.IsNullOrEmpty(query))
        {
            Writer.WriteWarning("No search term provided.", scope: nameof(SearchCommand));
            return Ok();
        }
        switch (searchType)
        {
            case "track":
                var tracks = SearchService.Default.SearchTracks(query, limit);
                if(AppendCommand.AppendMode) SelectedManager.Default.AppendToSelected(tracks);
                else SelectedManager.Default.UpdateSelected(tracks);
                CustomListService.ShowSelectedTracks(tracks, Writer);
                break;

            case "album":
                var albums = SearchService.Default.SearchAlbums(query, limit);
                if (albums.Count > 1)
                {
                    var selectedAlbums = CustomListService.ShowSelectFromList("Select a album!", albums, Writer, true, (info, s) => info.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
                    var albumTracks = new List<TrackObject>();
                    foreach (var albumTrack in selectedAlbums.Select(album => TrackService.Default.GetAlbumTracks(album.Id)).Where(artistTracks => artistTracks.Count != 0))
                    {
                        albumTracks.AddRange(albumTrack);
                    }
                    CustomListService.ShowSelectedTracks(albumTracks, Writer);
                }
                break;

            case "artist":
                var artists = SearchService.Default.SearchArtists(query, limit);
                var selectedArtists = CustomListService.ShowSelectFromList("Select artist(s)!", artists, Writer, true, (info, s) => info.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
                var artistTracks = new List<TrackObject>();
                foreach (var art in selectedArtists)
                {
                    var tracksByArtist = ArtistService.Default.GetTopTracks(art.Id);
                    if (tracksByArtist.Count == 0) continue;
                    artistTracks.AddRange(tracksByArtist);
                }
                CustomListService.ShowSelectedTracks(artistTracks, Writer);
                break;

            case "playlist":
                var playlists = SearchService.Default.SearchPlaylists(query, limit);
                if (!playlists.Any())
                {
                    Writer.WriteLine("No playlists found.");
                }
                else
                {
                    Writer.WriteTable(playlists.Select(p => new { p.Name, p.Owner, p.TrackCount }));
                }
                break;

            default:
                Writer.WriteLine($"Unknown search type: {searchType}");
                break;
        }
        return Ok();
    }
}