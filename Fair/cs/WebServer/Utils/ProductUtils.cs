namespace Uccs.Fair;

public static class ProductUtils
{
	public static string GetTitle(Product product)
	{
		//ProductField titleField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title);
		//return titleField != null ? titleField.Value.ToString() : null;

		// TODO: should be changed in release version.

		ProductField titleField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title);
		var title = titleField != null ? titleField.Value.ToString() : null;
		return !string.IsNullOrEmpty(title) ? title : product.Id.ToString();
	}

	public static string GetDescription(Product product)
	{
		//ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description);
		//return descriptionField != null ? descriptionField.Value.ToString() : null;

		// TODO: should be changed in release version.

		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description);
		var description = descriptionField != null ? descriptionField.Value.ToString() : null;
		return !string.IsNullOrEmpty(description) ? description : $"({product.Id} {product.Flags} {product.AuthorId})";
	}
}
