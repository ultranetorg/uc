using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesController
(
	ILogger<SitesController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ISiteSearchQueryValidator siteSearchQueryValidator,
	ISearchQueryValidator searchQueryValidator,
	SitesService sitesService,
	SearchService searchService
) : BaseController
{
	[HttpGet("default")]
	public IEnumerable<SiteBaseModel> Default(CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called without parameters", nameof(SitesController), nameof(SitesController.Default));

		return sitesService.GetDefaultSites(cancellationToken);
	}

	[HttpGet("{siteId}/publishers")]
	public IEnumerable<AuthorBaseAvatarModel> GetPublishers(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}", nameof(SitesController), nameof(GetPublishers), siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetPublishers(siteId, cancellationToken);
	}

	[HttpGet("{siteId}/moderators")]
	public IEnumerable<AccountBaseModel> GetModerators(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}", nameof(SitesController), nameof(GetModerators), siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetModerators(siteId, cancellationToken);
	}

	[HttpGet]
	public IEnumerable<SiteBaseModel> Search([FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}, {Page}", nameof(SitesController), nameof(Search), query, page);

		paginationValidator.Validate(page);
		siteSearchQueryValidator.Validate(query);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<SiteBaseModel> sites = searchService.SearchSites(query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(sites.Items, pageValue, pageSizeValue, sites.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<SiteSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}", nameof(SitesController), nameof(SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteSites(query, 0, SiteConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}", nameof(SitesController), nameof(Get), siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetSite(siteId);
	}
}
