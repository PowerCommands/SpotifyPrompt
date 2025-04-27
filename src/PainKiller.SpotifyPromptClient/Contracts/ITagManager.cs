using PainKiller.SpotifyPromptClient.DomainObjects.Data;

namespace PainKiller.SpotifyPromptClient.Contracts;

public interface ITagManager
{
    void AddTags<TKey, TEntity>(SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new();
    void AddTags<TKey, TEntity>(List<TEntity> items, SpotifyObjectStorage<TKey, TEntity> store, string filterTitle, Func<TEntity, string> nameSelector, Func<TEntity, string> idSelector, string filter) where TKey : IDataObjects<TEntity>, new() where TEntity : class, IContainsTags, new();
}