using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
public class ObjectStorage<T, TItem> where T : IDataObjects<TItem>, new() where TItem : class, new()
{
    private T _dataObject = StorageService<T>.Service.GetObject();

    public List<TItem> GetItems(bool reload = false)
    {
        if (reload) _dataObject = StorageService<T>.Service.GetObject();
        return _dataObject.Items;
    }
    public void SaveItems(List<TItem> items)
    {
        _dataObject.Items = items;
        _dataObject.LastUpdated = DateTime.Now;
        StorageService<T>.Service.StoreObject(_dataObject);
    }
    public void Insert(TItem item, Func<TItem, bool> match, bool saveToFile = true)
    {
        var existing = _dataObject.Items.FirstOrDefault(match);
        if (existing != null) _dataObject.Items.Remove(existing);
        _dataObject.Items.Add(item);
        _dataObject.LastUpdated = DateTime.Now;
        if(saveToFile) StorageService<T>.Service.StoreObject(_dataObject);
    }
    public bool Remove(Func<TItem, bool> match, bool saveToFile = true)
    {
        var existing = _dataObject.Items.FirstOrDefault(match);
        if (existing == null) return false;

        _dataObject.Items.Remove(existing);
        _dataObject.LastUpdated = DateTime.Now;
        StorageService<T>.Service.StoreObject(_dataObject);
        return true;
    }
    public void Save() => StorageService<T>.Service.StoreObject(_dataObject);
}