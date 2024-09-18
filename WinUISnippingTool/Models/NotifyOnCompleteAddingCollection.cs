using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace WinUISnippingTool.Models
{
    internal sealed class NotifyOnCompleteAddingCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
