namespace Uccs.Web.Pagination;

public class TotalItemsResult<T> where T : class
{
	public static TotalItemsResult<T> Empty = new TotalItemsResult<T>() { Items = null, TotalItems = 0 };

	public IEnumerable<T>? Items { get; init; } = [];

	public int TotalItems { get; init; }
}
