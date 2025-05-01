namespace Uccs.Fair;

public class SearchLightContext<T> where T : class
{
	public string Query { get; set; }

	public IList<T> Items { get; set; }

	public int TotalItems { get; set; }
}
