using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationModel : PublicationBaseModel
{
	public string[] Sections { get; set; }

	public PublicationStatus Status { get; set; }

	public string CategoryId { get; set; }

	// Account
	public string CreatorId { get; set; }

	public ProductField[] ProductFields { get; set; }
	public int ProductUpdated { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }

	public IEnumerable<ReviewModel> Reviews { get; set; }

	public PublicationModel(Publication publication, Product product, Author author) : base(publication.Id, product)
	{
// TODO by Maximion		Sections = publication.Fields;
		Status = publication.Status;

		CategoryId = publication.Category.ToString();

		CreatorId = publication.Creator.ToString();

		ProductId = publication.Product.ToString();
		ProductFields = product.Fields;
		ProductUpdated = product.Updated.Days;

		AuthorId = author.Id.ToString();
		AuthorTitle = author.Title;
	}
}
