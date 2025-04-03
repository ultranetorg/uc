using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesController
(
	ILogger<SitesController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	ISitesService sitesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<SiteBaseModel> Search([FromQuery] PaginationRequest pagination, [FromQuery] string? search)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{Pagination}}, {{Search}}", pagination, search);

		// TODO: validate search string: title
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<SiteBaseModel> sites = sitesService.SearchNonOptimized(page, pageSize, search);

		return this.OkPaged(sites.Items, page, pageSize, sites.TotalItems);
	}

	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{SiteId}}", siteId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());

		return sitesService.GetSite(siteId);
	}
}
