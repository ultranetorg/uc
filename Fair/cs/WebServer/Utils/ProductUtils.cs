namespace Uccs.Fair;

public static class ProductUtils
{
	public static string GetTitle(Product product)
	{
		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title);
		return descriptionField != null ? descriptionField.Value.ToString() : null;
	}

	public static string GetDescription(Product product)
	{
		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description);
		return descriptionField != null ? descriptionField.Value.ToString() : null;
	}
}
