namespace Uccs.Fair;

public class PublicationDetailsModel(Publication publication, Product product, Author author, Category category)
	: PublicationExtendedModel(publication, product, author, category)
{
	public int ReviewsCount { get; set;} = publication.Reviews.Count();

	public string CreatorId { get; set; } = publication.Creator.ToString();

	public string Description { get; set; } = PublicationUtils.GetDescription(publication, product);	

	public int ProductUpdated { get; set; } = product.Updated.Days;
}
