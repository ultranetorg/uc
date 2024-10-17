// ReSharper disable once CheckNamespace
namespace System.Collections.ObjectModel;

internal static class ObservableCollectionExtensions
{
	internal static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> items)
	{
		foreach (T? item in items)
		{
			source.Add(item);
		}
	}

	internal static void ReplaceAll<T>(this ObservableCollection<T> source, IEnumerable<T> items)
	{
		source.Clear();
		source.AddRange(items);
	}
}
