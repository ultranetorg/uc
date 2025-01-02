namespace Uccs.WebUI.Pagination;

public class TotalItemsResult<T> where T : class
{
	public IEnumerable<T> Items { get; init; } = [];

	public int TotalItems { get; init; }
}
