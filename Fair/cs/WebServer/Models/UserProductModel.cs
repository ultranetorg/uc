namespace Uccs.Fair;

public class UserProductModel(Product product)
{
	public AutoId Id { get; set; } = product.Id;

	public string Title { get; set; }

	public Time Updated { get; set; } = product.Updated;
}
