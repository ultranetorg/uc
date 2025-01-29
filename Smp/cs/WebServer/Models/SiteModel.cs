using Uccs.Web.Pagination;

namespace Uccs.Smp;

public class SiteModel
{
	public string Id { get; set; }

	public SiteType Type { get; set; }

	public string Title { get; set; }

	// public EntityId[] Owners { get; set; }

	public CategorySubModel[] Categories { get; set; }
}
