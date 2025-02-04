using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SiteModel : SiteBaseModel
{
	// public EntityId[] Owners { get; set; }

	public AccountModel[] Moderators { get; set; }

	public CategorySubModel[] Categories { get; set; }
}
