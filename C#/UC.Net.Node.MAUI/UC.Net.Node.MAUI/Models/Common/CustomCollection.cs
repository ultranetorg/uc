using System.Collections.Specialized;

namespace UC.Net.Node.MAUI.Models;

public class CustomCollection<T> : ObservableCollection<T>
{
    public CustomCollection(IEnumerable<T> list)
    { 
        AddRange(list);
    }

    public CustomCollection()
    {
    }

    public void AddRange(IEnumerable<T> list)
    {
        try
        {
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			foreach (T item in list)
			{
				Add(item);
			}
        }
        catch // Exception ex
        {
        }
    }
}
