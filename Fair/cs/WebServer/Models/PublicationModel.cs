using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationModel : PublicationBaseModel
{
	public string CategoryId { get; set; }

	// References to Account.
	public string CreatorId { get; set; }

	public IEnumerable<ProductFieldModel> ProductFields { get; set; }
	public int ProductUpdated { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }

	public IEnumerable<ReviewModel> Reviews { get; set; }

	public PublicationModel(Publication publication, Product product, Author author) : base(publication.Id, product)
	{
		CategoryId = publication.Category.ToString();

		CreatorId = publication.Creator.ToString();

		ProductId = publication.Product.ToString();
		ProductUpdated = product.Updated.Days;

		AuthorId = author.Id.ToString();
		AuthorTitle = author.Title;
	}
}
