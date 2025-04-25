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

	public static string GetTitle(Publication publication, Product product)
	{
		if (publication.Fields.All(x => x.Name != ProductField.Title))
		{
			return product.Id.ToString();
		}

		int version = publication.Fields.FirstOrDefault(x => x.Name == ProductField.Title).Version;
		byte[] bytes = product.Fields.FirstOrDefault(x => x.Name == ProductField.Title)?.Versions.FirstOrDefault(x => x.Version == version)?.Value;
		return Encoding.UTF8.GetString(bytes);
	}

	public static string? GetDescription(Publication publication, Product product)
	{
		if (publication.Fields.All(x => x.Name != ProductField.Description))
		{
			return product.Id.ToString() + " Description";
		}

		int version = publication.Fields.FirstOrDefault(x => x.Name == ProductField.Description).Version;
		byte[] bytes = product.Fields.FirstOrDefault(x => x.Name == ProductField.Description)?.Versions.FirstOrDefault(x => x.Version == version)?.Value;
		return Encoding.UTF8.GetString(bytes);	
	}
}
