using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;
using Uccs.Web.Utilities;

namespace Uccs.Smp;

public class SitesController
(
	ILogger<SitesController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	ISitesService sitesService
) : BaseController
{
	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{SiteId}}", siteId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());

		SiteModel site = sitesService.Find(siteId);
		If.Value(site).IsNull().Throw(() => new EntityNotFoundException(nameof(Site).ToLower(), siteId));

		return site;
	}

	[HttpGet("{siteId}/publications")]
	public IEnumerable<SitePublicationModel> Search(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string name)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Search)} method called with {{SiteId}}, {{Pagination}}, {{Name}}", siteId, pagination, name);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: name
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SitePublicationModel> products = sitesService.SearchPublications(siteId, page, pageSize, name);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
