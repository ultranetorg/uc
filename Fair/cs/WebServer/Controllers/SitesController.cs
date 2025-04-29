using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesController
(
	ILogger<SitesController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	ISearchQueryValidator searchQueryValidator,
	ISitesService sitesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<SiteBaseModel> Search([FromQuery] int? page, [FromQuery] string? search)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Search)} method called with {{Page}}, {{Search}}", page, search);

		searchQueryValidator.Validate(search);
		paginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<SiteBaseModel> sites = sitesService.SearchNotOptimized(pageValue, pageSizeValue, search);

		return this.OkPaged(sites.Items, pageValue, pageSizeValue, sites.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<SiteSearchLightModel> SearchLight([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.SearchLight)} method called with {{Query}}", query);

		searchQueryValidator.Validate(query);

		TotalItemsResult<SiteSearchLightModel> sites = sitesService.SearchLightNotOpmized(query, cancellationToken);

		return this.OkPaged(sites.Items, sites.TotalItems);
	}

	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{SiteId}}", siteId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetSite(siteId);
	}
}
