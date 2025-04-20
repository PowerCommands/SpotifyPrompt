using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(
    description: "Spotify - Enrich your items with tags.",
    options: ["artist", "album", "playlist", "filter-tag-missing"],
    examples: ["//Add a tag to artist","tags --artist","//Add a tag to album","tags --album","//Add a tag to playlist","tags --playlist","//Show only does who misses a tag","tags --filter-tag-missing"]
)]
public class TagsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly ObjectStorage<Albums, Album> _albumStore       = new();
    private readonly ObjectStorage<Artists, ArtistSimplified> _artistStore     = new();
    private readonly ObjectStorage<Playlists, PlaylistInfo> _playlistStore = new();

    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("artist"))
            AddTags(_artistStore, "Filter artists to tag", a => a.Name, a => a.Id, input.HasOption("filter-tag-missing"));
        else if (input.HasOption("album"))
            AddTags(_albumStore, "Filter albums to tag", a => a.Name, a => a.Id, input.HasOption("filter-tag-missing"));
        else if (input.HasOption("playlist"))
            AddTags(_playlistStore, "Filter playlists to tag", p => p.Name, p => p.Id, input.HasOption("filter-tag-missing"));
        return Ok();
    }
    private void AddTags<TKey, TEntity>(ObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, bool tagMissing) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new()
    {
        var items = tagMissing ? store.GetItems().Where(t => string.IsNullOrEmpty(t.Tags)).ToList() : store.GetItems();
        var names = items.Select(nameSelector).ToList();
        var selected = ListService.FilteredListDialog(filterTitle, names);

        foreach (var idx in selected)
        {
            var entity = items[idx.Key];
            var tags = new List<string>();
            Genres choice;

            while ((choice = ToolbarService.NavigateToolbar<Genres>()) != Genres.End)
            {
                if (choice == Genres.Custom)
                {
                    var custom = DialogService.QuestionAnswerDialog("Input your custom tag?");
                    if (!string.IsNullOrWhiteSpace(custom))
                        tags.Add(custom);
                }
                else
                {
                    tags.Add(choice.ToString());
                }
            }

            if (tags.Count == 0)
                continue;
            entity.Tags = string.Join(',', tags);
            store.Insert(entity, e => idSelector(e) == idSelector(entity), saveToFile: false);
        }
        store.Save();
    }
}