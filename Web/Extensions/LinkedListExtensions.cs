namespace Uccs.Web.Extensions;

public static class LinkedListExtensions
{
	public static void AppendRange<T>(this LinkedList<T> source, IEnumerable<T> items)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(items);

		foreach (T item in items)
		{
			source.AddLast(item);
		}
	}
}
