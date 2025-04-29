namespace Uccs.Fair;

public static class CategoryTableExtensions
{
	public static IEnumerable<Category> FindBySiteId(this CategoryTable table, AutoId siteId)
	{
		return table.Clusters.SelectMany(x => x.Buckets.SelectMany(x => x.Entries)).Where(x => x.Site == siteId);
	}
}
