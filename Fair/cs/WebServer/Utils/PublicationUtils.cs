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

	public static string? GetTitle(Publication publication, Product product) => FindProductField(publication, product, Token.Title)?.AsUtf8;

	public static string? GetLatestTitle(Product product) => FindLatestField(product, Token.Title)?.AsUtf8;

	public static string? GetLatestDescription(Product product) => FindLatestField(product, Token.DescriptionMinimal)?.AsUtf8;

	public static string? GetDescription(Publication publication, Product product) => FindProductField(publication, product, Token.Description)?.AsUtf8;

	public static AutoId? GetLogo(Publication publication, Product product) => FindProductField(publication, product, Token.Logo)?.AsAutoId;

	public static AutoId? GetLatestLogo(Product product) => FindLatestField(product, Token.Logo)?.AsAutoId;

	static FieldValue? FindProductField(Publication publication, Product product, Token fieldName)
	{
		var version = product.Versions.FirstOrDefault(i => i.Id == publication.ProductVersion);

		return version.Fields.FirstOrDefault(x => x.Name == fieldName);
	}

	static FieldValue? FindLatestField(Product product, Token fieldName) => product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields.FirstOrDefault(x => x.Name == fieldName);
}
