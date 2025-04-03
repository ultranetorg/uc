namespace Uccs.Fair;

internal class SearchContext<T> where T : class
{
	public int Page { get; set; }
	public int PageSize { get; set; }

	public int TotalItems { get; set; }
	public IList<T> Items { get; set; }
}
