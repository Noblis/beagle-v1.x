using System.Collections.ObjectModel;

namespace Supermodel.Client.Backend.Utils;

public static class ObservableCollectionExtensions
{
    public static int RemoveAllWhere<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
    {
        var itemsToRemove = coll.Where(condition).ToList();
        foreach (var itemToRemove in itemsToRemove) coll.Remove(itemToRemove);
        return itemsToRemove.Count;
    }
}