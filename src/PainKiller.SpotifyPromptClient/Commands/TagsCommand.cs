using System.Collections.Concurrent;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;
using PainKiller.SpotifyPromptClient.Utils;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(
    description: "Spotify - Enrich your items with tags.",
        options: ["auto", "ai", "filter"],
    suggestions: ["artist", "album", "playlist", "track"],
       examples: ["//Add a tag to artist", "tags --artist", "//Add a tag to album", "tags --album", "//Add a tag to playlist", "tags --playlist", "//Show only does who misses a tag", "tags --filter-tag-missing"]
)]
public class TagsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly SpotifyObjectStorage<Albums, Album> _albumStore = new();
    private readonly SpotifyObjectStorage<Artists, ArtistSimplified> _artistStore = new();
    private readonly SpotifyObjectStorage<Playlists, PlaylistInfo> _playlistStore = new();

    public override RunResult Run(ICommandLineInput input)
    {
        var mode = this.GetSuggestion(input.Arguments.FirstOrDefault(), "artist");
        if (input.HasOption("ai")) return AiTagArtists();
        if (input.HasOption("auto") && mode == "track") return AutoTagTracks();
        if (input.HasOption("auto") && mode == "artist") return AutoTagArtists();

        var filter = input.GetOptionValue("filter");

        var artists = StorageService<Artists>.Service.GetObject().Items;
        var albums = StorageService<Albums>.Service.GetObject().Items;
        var playlists = StorageService<Playlists>.Service.GetObject().Items;

        var taggedArtists = artists.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();
        var taggedAlbums = albums.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();
        var taggedPlaylists = playlists.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();

        Writer.WriteDescription("Artists:", $"{taggedArtists.Count} of {artists.Count}");
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
            trackStorage.Insert(track, t => t.Id == track.Id, saveToFile: false);
            Writer.WriteLine($"Auto tagged «{track.Name}» with [{track.Tags}]");
        }
        trackStorage.Save();
        Writer.WriteSuccessLine("Auto‑tagging of tracks done and updates persisted.");
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
            if (!string.IsNullOrEmpty(artist.Tags) && artist.Tags.Trim() != "Unknown")
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
    private RunResult AiTagArtists()
    {
        Writer.WriteHeadLine("This function is only for test, no artist are actually updated.");
        var artistStorage = new SpotifyObjectStorage<Artists, ArtistSimplified>();
        var artists = artistStorage.GetItems();

        var aiMatch = new ConcurrentBag<ArtistSimplified>();

        var config = Configuration.Core.Modules.Ollama;
        var ai = new AIManager(config.BaseAddress, config.Port, config.Model);
        var statement = DialogService.QuestionAnswerDialog("Input your AI statement, all artist matching your statement will be shown.\nQuery always begin with artist name, you input the rest, example:\nis a swedish artist or band\n:");

        var writerLock = new object();
        Parallel.ForEach(artists, artistSimplified =>
        {
            var info = WikipediaService.Default.TryFetchWikipediaIntro(artistSimplified.Name);
            var prediction = ai.GetPredictionToQuery($"{artistSimplified.Name} {statement}", info);
            if (prediction)
            {
                aiMatch.Add(artistSimplified);
                lock (writerLock)
                {
                    Writer.WriteLine($"Artist {artistSimplified.Name} match your statement");
                }
            }
        });
        if (aiMatch.Count > 0)
        {
            artists.Clear();
            artists.AddRange(aiMatch);
        }
        return Ok();
    }
}