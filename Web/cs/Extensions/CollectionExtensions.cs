using Ardalis.GuardClauses;

namespace Uccs.Web.Extensions;

public static class CollectionExtensions
{
	public static List<List<T>> SliceIntoBatches<T>(this ICollection<T> collection, int batchSize)
	{
		Guard.Against.Negative(batchSize);

		if (collection.Count == 0)
		{
			return [];
		}

		return SliceIntoBatchesInternal(collection, batchSize);
	}

	private static List<List<T>> SliceIntoBatchesInternal<T>(ICollection<T> collection, int batchSize)
	{
		int batchesCount = (collection.Count / batchSize) + 1;

		List<List<T>> batchList = new(batchesCount);
		List<T> batch = new(batchSize);

		foreach (T item in collection)
		{
			if (batch.Count == batchSize)
			{
				batchList.Add(batch);
				batch = new List<T>(batchSize);
			}

			batch.Add(item);
		}

		batchList.Add(batch);

		return batchList;
	}
}
