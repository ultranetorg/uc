using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISitesService
{
	TotalItemsResult<SiteBaseModel> SearchNonOptimized([NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string? search);

	SiteModel GetSite([NotEmpty] string siteId);
}
