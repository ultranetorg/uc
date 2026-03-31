namespace Uccs.Fair;

public class PublicationDetailsModel(Publication publication, Product product, Author author, Category category, byte[]? logo)
	: PublicationExtendedModel(publication, product, author, category, logo)
{
	public ProductType ProductType { get; } = product.Type;

	public IEnumerable<FieldValueModel> ProductFields { get; set; }

	public int Rating { get; set; } = publication.Rating;

	public int ReviewsCount { get; set;} = publication.Reviews.Count();

	public string Description { get; set; } = PublicationUtils.GetDescription(publication, product);	

	public int Updated { get; set; } = product.Updated.Hours;
}
