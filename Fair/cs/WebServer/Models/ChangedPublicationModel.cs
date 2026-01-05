namespace Uccs.Fair;

public class ChangedPublicationModel(string publicationId, Product product, int publicationVersion, FairUser account, Category category, AutoId logoId)
{
	public string Id { get; } = publicationId;

	public PublicationImageBaseModel Publication { get; } = new PublicationImageBaseModel(product, logoId);
	public AccountBaseAvatarModel Author { get; } = new AccountBaseAvatarModel(account);

	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;

	public int CurrentVersion { get; } = publicationVersion;
	public int LatestVersion { get; } = product.Versions.Length - 1;
}
