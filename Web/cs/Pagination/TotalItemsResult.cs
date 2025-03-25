namespace Uccs.Web.Pagination;

public class TotalItemsResult<T>
	where T : class
{
	public static TotalItemsResult<T> Empty = new TotalItemsResult<T>() { Items = null, TotalItems = 0 };

	public IEnumerable<T>? Items { get; set; } = [];

	public int TotalItems { get; set; }
}
