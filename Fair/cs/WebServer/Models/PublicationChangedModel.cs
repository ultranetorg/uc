namespace Uccs.Fair;

public class PublicationChangedModel(Product product, int publicationVersion, FairAccount account, Category category, byte[]? logo) : PublicationBaseSiteModel(product, account, logo)
{
	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;

	public int CurrentVersion { get; } = publicationVersion;
	public int LatestVersion { get; } = product.Versions.Length - 1;
}
