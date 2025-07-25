namespace Uccs.Fair;

public class PublicationDetailsModel(Publication publication, Product product, Author author, Category category, byte[]? logo, byte[]? authorAvatar)
	: PublicationExtendedModel(publication, product, author, category, logo)
{
	public byte[] AuthorAvatar { get; set; } = authorAvatar;

	public int Rating { get; set; } = publication.Rating;

	public int ReviewsCount { get; set;} = publication.Reviews.Count();

	public string Description { get; set; } = PublicationUtils.GetDescription(publication, product);	

	public int ProductUpdated { get; set; } = product.Updated.Days;
}
