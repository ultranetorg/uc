using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationDetailsModel : PublicationBaseModel
{
	public string CategoryId { get; set; }

	// References to Account.
	public string CreatorId { get; set; }

	public IEnumerable<ProductFieldModel> ProductFields { get; set; }
	public int ProductUpdated { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }
	public string AuthorNickname { get; set; }

	public PublicationDetailsModel(Publication publication, Product product, Author author) : base(publication, product)
	{
		CategoryId = publication.Category.ToString();

		CreatorId = publication.Creator.ToString();

		ProductId = publication.Product.ToString();
		ProductUpdated = product.Updated.Days;

		AuthorId = author.Id.ToString();
		AuthorTitle = author.Title;
		AuthorNickname = author.Nickname;
	}
}
