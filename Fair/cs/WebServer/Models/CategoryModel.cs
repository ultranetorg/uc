using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class CategoryModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public string SiteId { get; set; }

	public string ParentId { get; set; }
	public string ParentTitle { get; set; }

	public CategorySubModel[] Categories { get; set; }
	public CategoryPublicationModel[] Publications { get; set; }
}
