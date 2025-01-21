namespace Uccs.Fair;

public class ProductModel
{
	public string Id { get; set; }
	public string Name { get; set; }

	public string AuthorId { get; set; }
	public string AuthorName { get; set; }

	public ProductFlags Flags { get; set; }

	public ProductField[] Fields { get; set; }

	public int Updated { get; set; }
}
