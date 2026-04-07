namespace Uccs.Fair;

public static class PublicationUtils
{
	public static string? GetTitle(Publication publication, Product product) => FindProductField(publication, product, Token.Title)?.AsUtf8;

	public static string? GetLatestTitle(Product product) => FindLatestField(product, Token.Title)?.AsUtf8;

	public static AutoId? GetLogo(Publication publication, Product product) => FindProductField(publication, product, Token.Logo)?.AsAutoId;

	public static AutoId? GetLatestLogo(Product product) => FindLatestField(product, Token.Logo)?.AsAutoId;

	static FieldValue? FindProductField(Publication publication, Product product, Token fieldName)
	{
		var version = product.Versions.FirstOrDefault(i => i.Id == publication.ProductVersion);

		return version.Fields.FirstOrDefault(x => x.Name == fieldName);
	}

	static FieldValue? FindLatestField(Product product, Token fieldName) => product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields.FirstOrDefault(x => x.Name == fieldName);

	// TODO: Should be removed.
	public static string GetUrl(Publication publication)
	{
		// TODO: implement method.
		return "https://google.com/publications/" + publication.Id.ToString();
	}
}
