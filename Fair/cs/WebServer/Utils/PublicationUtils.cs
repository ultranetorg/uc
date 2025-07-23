using System.Text;

namespace Uccs.Fair;

public static class PublicationUtils
{
	public static string GetUrl(Publication publication)
	{
		// TODO: implement method.
		return "https://google.com/publications/" + publication.Id.ToString();
	}

	public static string[] GetSupportedOSes(Publication publication, Product product)
	{
		return ["Windows", "macOS", "Linux", "Android"];
	}

	public static string[] GetSupportedOSesExtended(Publication publication, Product product)
	{
		return ["Windows 3.11", "Windows 95", "Windows 98", "macOS Sierra", "Ubuntu", "Xubuntu", "Android 11", "Android 12", "Android 13", "Android 14"];
	}

	public static string? GetTitle(Publication publication, Product product) =>
		FindProductField(publication, product, ProductFieldName.Title)?.AsUtf8;

	public static string? GetDescription(Publication publication, Product product) =>
		FindProductField(publication, product, ProductFieldName.Description)?.AsUtf8;

	public static AutoId? GetLogo(Publication publication, Product product) =>
		FindProductField(publication, product, ProductFieldName.Logo)?.AsAutoId;


	static ProductFieldVersion? FindProductField(Publication publication, Product product, ProductFieldName fieldName)
	{
		int index = Array.FindIndex(publication.Fields, x => x.Field == fieldName);
		if (index == -1)
		{
			return null;
		}

		int version = publication.Fields[index].Version;
		return product.Fields.First(x => x.Name == fieldName).Versions.First(x => x.Version == version);
	}
}
