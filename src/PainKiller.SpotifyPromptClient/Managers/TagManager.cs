using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Managers;

public class TagManager : ITagManager
{
    private readonly IConsoleWriter _writer = ConsoleService.Writer;
    private TagManager() { }
    private static readonly Lazy<ITagManager> Instance = new(() => new TagManager());
    public static ITagManager Default => Instance.Value;

    public void AddTags<TKey, TEntity>(SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new()
    {
        var items = store.GetItems().Where(i => string.IsNullOrEmpty(i.Tags.Trim())).ToList();
        AddTags(items, store, filterTitle, nameSelector, idSelector, filter);
    }
    public void AddTags<TKey, TEntity>(List<TEntity> items, SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new()
    {
        if(!string.IsNullOrEmpty("filter")) items = items.Where(t => t.Tags.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var names = items.Select(nameSelector).ToList();
        var selected = ListService.FilteredListDialog(filterTitle, names);

        var choice = Genres.NotSet;
        foreach (var idx in selected)
        {
            if(choice == Genres.End) break;
            _writer.Clear();
            var entity = items[idx.Key];
            ConsoleService.WriteCenteredText("Add tags...",entity.Name);
            var description = WikipediaService.Default.TryFetchWikipediaIntro(entity.Name);
            if (!string.IsNullOrWhiteSpace(description)) _writer.WriteDescription("Description", description);
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
            entity.Tags = string.Join(',', tags).ToLower();
            store.Insert(entity, e => idSelector(e) == idSelector(entity));
        }
        _writer.WriteSuccessLine("All items have been tagged.");
    }
}