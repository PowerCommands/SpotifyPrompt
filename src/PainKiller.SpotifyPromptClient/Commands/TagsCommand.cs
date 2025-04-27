using System.Text.RegularExpressions;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Utils;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(
    description: "Spotify - Enrich your items with tags.",
        options: ["repair","auto","filter"],
    suggestions: ["artist", "album", "playlist", "track"],
       examples: ["//Add a tag to artist","tags --artist","//Add a tag to album","tags --album","//Add a tag to playlist","tags --playlist","//Show only does who misses a tag","tags --filter-tag-missing"]
)]
public class TagsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly SpotifyObjectStorage<Albums, Album> _albumStore = new();
    private readonly SpotifyObjectStorage<Artists, ArtistSimplified> _artistStore = new();
    private readonly SpotifyObjectStorage<Playlists, PlaylistInfo> _playlistStore = new();

    public override RunResult Run(ICommandLineInput input)
    {
        var mode = this.GetSuggestion(input.Arguments.FirstOrDefault(), "artist");
        if (input.HasOption("auto") && mode == "track") return AutoTagTracks();
        if (input.HasOption("auto") && mode == "artist") return AutoTagArtists();
        if (input.HasOption("repair") && mode == "artist") return RepairArtistTags();

        var filter = input.GetOptionValue("filter");

        var artists = StorageService<Artists>.Service.GetObject().Items;
        var albums = StorageService<Albums>.Service.GetObject().Items;
        var playlists = StorageService<Playlists>.Service.GetObject().Items;

        var taggedArtists = artists.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();
        var taggedAlbums = albums.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();
        var taggedPlaylists = playlists.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();

        Writer.WriteDescription("Artists:",$"{taggedArtists.Count} of {artists.Count}");
        Writer.WriteDescription("Albums:", $"{taggedAlbums.Count} of {albums.Count}");
        Writer.WriteDescription("Playlists:", $"{taggedPlaylists.Count} of {playlists.Count}");

        Console.ReadLine();

        var tagService = TagManager.Default;
        if (mode == "artist") tagService.AddTags(_artistStore, "Filter artists to tag", a => a.Name, a => a.Id, filter);
        else if (mode == "album") tagService.AddTags(_albumStore, "Filter albums to tag", a => a.Name, a => a.Id, filter);
        else if (mode == "playlist") tagService.AddTags(_playlistStore, "Filter playlists to tag", p => p.Name, p => p.Id, filter);
        return Ok();
    }
    private RunResult AutoTagTracks()
    {
        var artists = StorageService<Artists>.Service.GetObject().Items;
        var trackStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = trackStorage.GetItems().ToList();

        foreach (var track in tracks)
        {
            if (track.Artists == null || track.Artists.Count == 0) continue;
            var trackArtist = track.Artists.FirstOrDefault();
            if (trackArtist == null) 
                continue;
            var artist = artists.FirstOrDefault(a => a.Id == trackArtist.Id);
            if (artist == null || string.IsNullOrEmpty(artist.Tags)) continue;

            var tags = RepairTag(artist.Tags);
            track.Tags = string.Join(',', tags);
            trackStorage.Insert(track, t => t.Id == track.Id, saveToFile: false);

            Writer.WriteLine($"Auto tagged «{track.Name}» with [{track.Tags}]");
        }
        trackStorage.Save();
        Writer.WriteSuccessLine("Auto‑tagging of tracks done and updates persisted.");
        return Ok();
    }
    private RunResult RepairArtistTags()
    {
        var simpleArtists = StorageService<Artists>.Service.GetObject().Items;
        foreach (var artist in simpleArtists)
        {
            var repairedTags = RepairTag(artist.Tags);
            artist.Tags = string.Join(',', repairedTags);
            _artistStore.Insert(artist, a => a.Id == artist.Id, saveToFile: false);
        }
        _artistStore.Save();
        Writer.WriteSuccessLine("Repairing of artist tags done and updates persisted.");
        return Ok();
    }
    private RunResult AutoTagArtists()
    {
        var simpleArtists = StorageService<Artists>.Service.GetObject().Items;
        var aiConfig = Configuration.Core.Modules.Ollama;
        var aiManager = new AIManager(aiConfig.BaseAddress, aiConfig.Port, aiConfig.Model);
        var counter = 0;
        foreach (var artist in simpleArtists)
        {
            counter++;
            if (counter++ % 5 == 0)
            {
                aiManager.ClearMessages();
            }
            if (!string.IsNullOrEmpty(artist.Tags) && artist.Tags.Trim() !=  "Unknown")
                continue;
            try
            {
                var category = aiManager.GetCategory(artist.Name);
                Writer.WriteLine($"Category from {aiConfig.Model}: {category}");
                var genreEnum = GenreMapper.Map(category);
                var genreName = genreEnum.ToString().ToLower();
                artist.Tags += genreName;
                _artistStore.Insert(artist, a => a.Id == artist.Id);
                Writer.WriteLine($"Auto tagged «{artist.Name}» with [{artist.Tags}]");
            }
            catch (Exception e)
            {
                Writer.WriteError(e.Message, nameof(TagsCommand));
            }
        }
        Writer.WriteSuccessLine("Auto‑tagging of artists done and updates persisted.");
        return Ok();
    }
    public List<string> RepairTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) 
            return new();
        var exceptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hiphop",
            "rnb",
            "hardrock"
        };
        var parts = tag
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        var segments = new List<string>();
        for (int i = 0; i < parts.Count; i++)
        {
            var lower = parts[i].ToLowerInvariant();
            if (i < parts.Count - 1)
            {
                var nextLower = parts[i + 1].ToLowerInvariant();
                var combo    = lower + nextLower;
                if (exceptions.Contains(combo))
                {
                    segments.Add(combo);
                    i++; // hoppa över nästa del
                    continue;
                }
            }
            var splits = Regex.Split(parts[i], @"(?<=[a-z])(?=[A-Z])");
            foreach (var s in splits)
            {
                var clean = s.Trim().ToLowerInvariant();
                if (clean.Length > 0 && clean != "unknown")
                    segments.Add(clean);
            }
        }
        return segments.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}