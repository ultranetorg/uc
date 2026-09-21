namespace Uccs.Fair;

public class PublicationModel(Publication publication, Product product, Category category) : PublicationBaseModel(publication, product)
{
	public AutoId? LogoFileId { get; } = PublicationUtils.GetLogo(publication, product);
	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;

	public byte Rating { get; } = publication.Rating;
}