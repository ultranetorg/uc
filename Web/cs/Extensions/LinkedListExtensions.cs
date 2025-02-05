using Ardalis.GuardClauses;

namespace Uccs.Web.Extensions;

public static class LinkedListExtensions
{
	public static void AppendRange<T>(this LinkedList<T> source, IEnumerable<T> items)
	{
		Guard.Against.Null(source);
		Guard.Against.Null(items);

		foreach (T item in items)
		{
			source.AddLast(item);
		}
	}
}
