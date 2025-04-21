using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(
    description: "Spotify - Enrich your items with tags.",
    options: ["artist", "album", "playlist", "filter", "filter-tag-missing"],
    examples: ["//Add a tag to artist","tags --artist","//Add a tag to album","tags --album","//Add a tag to playlist","tags --playlist","//Show only does who misses a tag","tags --filter-tag-missing"]
)]
public class TagsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly SpotifyObjectStorage<Albums, Album> _albumStore       = new();
    private readonly SpotifyObjectStorage<Artists, ArtistSimplified> _artistStore     = new();
    private readonly SpotifyObjectStorage<Playlists, PlaylistInfo> _playlistStore = new();

    public override RunResult Run(ICommandLineInput input)
    {
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

        if (input.HasOption("artist")) AddTags(_artistStore, "Filter artists to tag", a => a.Name, a => a.Id, input.HasOption("filter-tag-missing"), input.GetOptionValue("filter"));
        else if (input.HasOption("album")) AddTags(_albumStore, "Filter albums to tag", a => a.Name, a => a.Id, input.HasOption("filter-tag-missing"), input.GetOptionValue("filter"));
        else if (input.HasOption("playlist")) AddTags(_playlistStore, "Filter playlists to tag", p => p.Name, p => p.Id, input.HasOption("filter-tag-missing"), input.GetOptionValue("filter"));
        return Ok();
    }
    private void AddTags<TKey, TEntity>(SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, bool tagMissing, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new()
    {
        var items = tagMissing ? store.GetItems().Where(t => string.IsNullOrEmpty(t.Tags)).ToList() : store.GetItems();
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
}