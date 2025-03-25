using System.Text;

namespace Uccs.Fair;

public static class ProductUtils
{
	public static string GetTitle(Product product, Publication publication)
	{
		if (publication.Fields.All(x => x.Name != ProductField.Title))
		{
			return product.Id.ToString();
		}

		int version = publication.Fields.FirstOrDefault(x => x.Name == ProductField.Title).Version;
		byte[] bytes = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title)?.Versions.FirstOrDefault(x => x.Version == version)?.Value;
		return Encoding.UTF8.GetString(bytes);
	}

	public static string? GetDescription(Product product, Publication publication)
	{
		if (publication.Fields.All(x => x.Name != ProductField.Description))
		{
			return product.Id.ToString() + " Description";
		}

		int version = publication.Fields.FirstOrDefault(x => x.Name == ProductField.Title).Version;
		byte[] bytes = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description)?.Versions.FirstOrDefault(x => x.Version == version)?.Value;
		return Encoding.UTF8.GetString(bytes);	
	}
}
