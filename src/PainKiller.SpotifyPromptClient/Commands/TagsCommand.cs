using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Utils;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(
    description: "Spotify - Enrich your items with tags.",
        options: ["auto","filter"],
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

        if (mode == "artist") AddTags(_artistStore, "Filter artists to tag", a => a.Name, a => a.Id, filter);
        else if (mode == "album") AddTags(_albumStore, "Filter albums to tag", a => a.Name, a => a.Id, filter);
        else if (mode == "playlist") AddTags(_playlistStore, "Filter playlists to tag", p => p.Name, p => p.Id, filter);
        return Ok();
    }
    private void AddTags<TKey, TEntity>(SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new()
    {
        var items = store.GetItems().Where(i => !string.IsNullOrEmpty(i.Tags)).ToList();
        if(!string.IsNullOrEmpty("filter")) items = items.Where(t => t.Tags.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var names = items.Select(nameSelector).ToList();
        var selected = ListService.FilteredListDialog(filterTitle, names);

        var choice = Genres.NotSet;
        foreach (var idx in selected)
        {
            if(choice == Genres.End) break;
            Writer.Clear();
            var entity = items[idx.Key];
            ConsoleService.WriteCenteredText("Add tags...",entity.Name);
            var description = WikipediaManager.Default.TryFetchWikipediaIntro(entity.Name);
            if (!string.IsNullOrWhiteSpace(description)) Writer.WriteDescription("Description", description);
            var tags = new List<string>();
            while ((choice = ToolbarService.NavigateToolbar<Genres>()) != Genres.Next)
            {
                if(choice == Genres.End) break;
                if (choice == Genres.Custom)
                {
                    var custom = DialogService.QuestionAnswerDialog("Input your custom tag?");
                    if (!string.IsNullOrWhiteSpace(custom))
                    {
                        tags.Add(custom);
                        continue;
                    }
                }
                tags.Add(choice.ToString());
            }
            if (tags.Count == 0)
                continue;
            entity.Tags = string.Join(',', tags);
            store.Insert(entity, e => idSelector(e) == idSelector(entity), saveToFile: false);
        }
        store.Save();
        Writer.WriteSuccessLine("Changes is persisted.");
    }
    private RunResult AutoTagTracks()
    {
        var simpleArtists = StorageService<Artists>.Service.GetObject().Items;
        var artists = ArtistManager.Default.GetArtists(simpleArtists.Select(a => a.Id)).ToList();
        var trackStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = trackStorage.GetItems().Where(t => string.IsNullOrEmpty(t.Tags)).ToList();

        foreach (var track in tracks)
        {
            if (track.Artists == null || track.Artists.Count == 0) continue;
            var trackArtist = track.Artists.FirstOrDefault();
            if (trackArtist == null) 
                continue;
            var artist = artists.FirstOrDefault(a => a.Id == trackArtist.Id);
            if (artist?.Genres == null || artist.Genres.Count == 0) 
                continue;
         
            var tags = new List<string>();
            foreach (var rawGenre in artist.Genres)
            {
                var genreEnum = GenreMapper.Map(rawGenre);
                var genreName = genreEnum.ToString();
                if (!tags.Contains(genreName))
                    tags.Add(genreName);
            }

            track.Tags = string.Join(',', tags);
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
            if (!string.IsNullOrEmpty(artist.Tags) && artist.Tags.Trim() !=  "Unknown")
                continue;
            try
            {
                var category = aiManager.GetCategory(artist.Name);
                Writer.WriteLine($"Category from {aiConfig.Model}: {category}");
                var genreEnum = GenreMapper.Map(category);
                var genreName = genreEnum.ToString();
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
}