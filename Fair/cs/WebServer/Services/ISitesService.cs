using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISitesService
{
	TotalItemsResult<SiteBaseModel> SearchNotOptimized([NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string? search);

	TotalItemsResult<SiteSearchLightModel> SearchLightNotOpmized([NotNull][NotEmpty] string? query, CancellationToken cancellationToken);

	SiteModel GetSite([NotEmpty] string siteId);
}
