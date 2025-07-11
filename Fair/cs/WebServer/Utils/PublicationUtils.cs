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

	public static string? GetTitle(Publication publication, Product product)
	{
		var index = Array.FindIndex(publication.Fields, x => x.Field == ProductFieldName.Title);
		if (index == -1)
		{
			return null;
		}


		int version = publication.Fields[index].Version;
		return product.Fields.First(x => x.Name == ProductFieldName.Title).Versions.First(x => x.Version == version).AsUtf8;
	}

	public static string? GetDescription(Publication publication, Product product)
	{
		var index = Array.FindIndex(publication.Fields, x => x.Field == ProductFieldName.Description);
		if(index == -1)
		{
			return null;
		}

		int version = publication.Fields[index].Version;
		return product.Fields.First(x => x.Name == ProductFieldName.Description).Versions.First(x => x.Version == version).AsUtf8;
	}
}
