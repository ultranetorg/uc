namespace Uccs.Fair;

public static class ProductUtils
{
	public static string GetTitle(Product product)
	{
		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == "Description");
		return descriptionField != null ? descriptionField.Value.ToString() : null;
	}
}
