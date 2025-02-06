namespace Uccs.Fair;

public class UserProductModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public int Updated { get; set; }

	public UserProductModel(Product product)
	{
		Id = product.Id.ToString();
		Title = ProductUtils.GetTitle(product);
		Updated = product.Updated.Days;
	}
}
