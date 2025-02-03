using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SiteModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	// public EntityId[] Owners { get; set; }

	public CategorySubModel[] Categories { get; set; }
}
