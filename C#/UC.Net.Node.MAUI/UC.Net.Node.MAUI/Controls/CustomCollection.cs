using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace UC.Net.Node.MAUI.Controls;

public class CustomCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification = false;

    public CustomCollection(IEnumerable<T> list)
    { 
        AddRange(list);
    }

    public CustomCollection()
    {
    }

    public void ReportItemChange(T item)
    {
        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, item, item, IndexOf(item));
        OnCollectionChanged(args);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        try
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }
        catch // Exception ex
        {
        }
    }

    public void AddRange(IEnumerable<T> list)
    {
        try
        {
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			_suppressNotification = true;

			foreach (T item in list)
			{
				Add(item);
			}

			_suppressNotification = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        catch // Exception ex
        {
        }
    }
}
