using Uccs.Web.Pagination;

namespace Uccs.Smp;

public class PublicationModel
{
	public string Id { get; set; }

	public string CategoryId { get; set; }

	public string CreatorId { get; set; }

	public string ProductId { get; set; }
	public string ProductName { get; set; }
	public ProductField[] ProductFields { get; set; }
	public int ProductUpdated { get; set; }
	public string ProductAuthorId { get; set; }
	public string ProductAuthorTitle { get; set; }

	public string[] Sections { get; set; }

	public CommentModel[] Comments { get; set; }
}
