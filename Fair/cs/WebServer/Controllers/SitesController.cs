using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class SitesController
(
	ILogger<SitesController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ISiteSearchQueryValidator siteSearchQueryValidator,
	ISearchQueryValidator searchQueryValidator,
	ISitesService sitesService,
	ISearchService searchService
) : BaseController
{
	[HttpGet("default")]
	public IEnumerable<SiteBaseModel> Default(CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Default)} method called without parameters");

		return sitesService.GetDefaultSites(cancellationToken);
	}

	[HttpGet]
	public IEnumerable<SiteBaseModel> Search([FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Search)} method called with {{Query}}, {{Page}}", query, page);

		paginationValidator.Validate(page);
		siteSearchQueryValidator.Validate(query);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		IEnumerable<SiteBaseModel> sites = searchService.SearchSites(query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(sites, pageValue, pageSizeValue);
	}

	[HttpGet("search")]
	public IEnumerable<SiteSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.SearchLite)} method called with {{Query}}", query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteSites(query, 0, Pagination.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{SiteId}}", siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetSite(siteId);
	}
}
