namespace Uccs.Smp;

public static class ProductUtils
{
	public static string GetTitle(Product product)
	{
		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Type == ProductProperty.Description);
		return descriptionField != null ? descriptionField.Value.ToString() : null;
	}
}
