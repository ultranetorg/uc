using System.Xml.Linq;
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
	public IEnumerable<SiteBaseModel> Search([FromQuery] PaginationRequest pagination, [FromQuery] string name)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{Pagination}}, {{Name}}", pagination, name);

		// TODO: validate search string: name
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SiteBaseModel> sites = sitesService.SearchNonOptimized(page, pageSize, name);

		return this.OkPaged(sites.Items, page, pageSize, sites.TotalItems);
	}

	[HttpGet("{siteId}")]
	public SiteModel Get(string siteId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{SiteId}}", siteId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());

		SiteModel site = sitesService.Find(siteId);
		if (site == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return site;
	}

	[HttpGet("{siteId}/authors/{authorId}")]
	public SiteAuthorModel GetAuthor(string siteId, string authorId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetAuthor)} method called with {{SiteId}}. {{AuthorId}}", siteId, authorId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(authorId, nameof(Author).ToLower());

		SiteAuthorModel siteAuthor = sitesService.FindAuthorNonOptimized(siteId, authorId);
		if (siteAuthor == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return siteAuthor;
	}

	[HttpGet("{siteId}/disputes")]
	public IEnumerable<DisputeModel> GetDisputes(string siteId, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetDisputes)} method called with {{SiteId}}, {{Pagination}}", siteId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<DisputeModel> disputes = sitesService.FindDisputesNonOptimized(siteId, page, pageSize);
		if (disputes == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return this.OkPaged(disputes.Items, page, pageSize, disputes.TotalItems);
	}

	[HttpGet("{siteId}/publications")]
	public IEnumerable<SitePublicationModel> SearchPublications(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string name)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.SearchPublications)} method called with {{SiteId}}, {{Pagination}}, {{Name}}", siteId, pagination, name);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: name
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SitePublicationModel> products = sitesService.SearchPublicationsNonOptimized(siteId, page, pageSize, name);
		if (products == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
