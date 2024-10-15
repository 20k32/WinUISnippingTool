using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

public sealed class NotifyOnCompletionCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> range)
    {
        foreach (var item in range)
        {
            Items.Add(item);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void RemoveRange(IEnumerable<T> range)
    {
        foreach(var item in range)
        {
            Items.Remove(item);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
