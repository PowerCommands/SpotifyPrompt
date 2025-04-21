using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;

namespace PainKiller.SpotifyPromptClient.DomainObjects.Data;

public class SpotifyObjectStorage<T, TItem> : ObjectStorage<T, TItem> where T : IDataObjects<TItem>, new() where TItem : class, IContainsTags, new()
{
    public override void Insert(TItem item, Func<TItem, bool> match, bool saveToFile = true)
    {
        var existing = DataObject.Items.FirstOrDefault(match);
        if (existing != null)
        {
            item.Tags = existing.Tags;
            DataObject.Items.Remove(existing);
        }
        DataObject.Items.Add(item);
        DataObject.LastUpdated = DateTime.Now;
        if(saveToFile) StorageService<T>.Service.StoreObject(DataObject);
    }
}