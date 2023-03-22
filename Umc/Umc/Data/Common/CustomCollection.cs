using System.Diagnostics;

namespace UC.Umc.Models;

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
        catch(Exception ex)
        {
			Debug.WriteLine(ex);
        }
    }
}
