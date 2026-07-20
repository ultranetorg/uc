using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesController
(
	ILogger<SitesController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	SiteSearchQueryValidator siteSearchQueryValidator,
	SearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator,
	SitesService sitesService,
	UsersService usersService,
	SearchService searchService
) : BaseController
{
	[HttpGet("default")]
	public IEnumerable<SiteBaseModel> Default(CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called without parameters", nameof(SitesController), nameof(SitesController.Default));

		return sitesService.GetDefaultSites(cancellationToken);
	}

	[HttpGet("{siteId}/users")]
	public IEnumerable<UserModel> GetUsers(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {SiteId}, {Pagination}", nameof(SitesController), nameof(GetUsers), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Store));
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserModel> result = usersService.GetSiteUsers(siteId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("{siteId}/users/search")]
	public IEnumerable<UserModel> SearchSiteUsers(string siteId, [FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {SiteId}, {Query}, {Limit}", nameof(SitesController), nameof(SearchSiteUsers), siteId, query, limit);

		autoIdValidator.Validate(siteId, nameof(Store));
		siteSearchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchSiteUsers(siteId, query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[HttpGet("{siteId}/publishers")]
	public IEnumerable<PublisherModel> GetPublishers(string siteId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {Search}, {Pagination}", nameof(SitesController), nameof(GetPublishers), siteId, search, pagination);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherModel> publishers = sitesService.GetPublishers(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(publishers.Items, page, pageSize, publishers.TotalItems);
	}

	[HttpGet("{siteId}/moderators")]
	public IEnumerable<ModeratorModel> GetModerators(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}", nameof(SitesController), nameof(GetModerators), siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());

		return sitesService.GetModerators(siteId, cancellationToken);
	}

	[HttpGet("{siteId}/policies")]
	public IEnumerable<PolicyModel> GetPolicies(string siteId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}", nameof(SitesController), nameof(GetPolicies), siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());

		return sitesService.GetPolicies(siteId);
	}

	[HttpGet]
	public IEnumerable<SiteBaseModel> Search([FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Page}", nameof(SitesController), nameof(Search), query, page);

		paginationValidator.Validate(page);
		siteSearchQueryValidator.Validate(query);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<SiteBaseModel> result = searchService.SearchSites(query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(result.Items, pageValue, pageSizeValue, result.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<SiteSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}", nameof(SitesController), nameof(SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteSites(query, 0, SiteConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("{siteId}")]
	public SiteModel GetDetails(string siteId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}", nameof(SitesController), nameof(GetDetails), siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());

		return sitesService.GetDetails(siteId);
	}
}
