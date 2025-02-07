namespace Uccs.Fair;

public static class ProductUtils
{
	public static string GetTitle(Product product)
	{
		//ProductField titleField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title);
		//return titleField != null ? titleField.Value.ToString() : null;

		// TODO: should be changed in release version.

		ProductField titleField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title);
// TODO by Maximion		var title = titleField != null ? titleField.Value.ToString() : null;
// TODO by Maximion		return !string.IsNullOrEmpty(title) ? title : product.Id.ToString();
		return null;	
	}

	public static string GetDescription(Product product)
	{
		//ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description);
		//return descriptionField != null ? descriptionField.Value.ToString() : null;

		// TODO: should be changed in release version.

		ProductField descriptionField = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description);
// TODO	by Maximion		var description = descriptionField != null ? descriptionField.Value.ToString() : null;
// TODO	by Maximion		return !string.IsNullOrEmpty(description) ? description : $"({product.Id} {product.Flags} {product.AuthorId})";
		return null;	
	}
}
