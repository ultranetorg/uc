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

	[HttpGet("{siteId}/publications")]
	public IEnumerable<SitePublicationModel> Search(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string name)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Search)} method called with {{SiteId}}, {{Pagination}}, {{Name}}", siteId, pagination, name);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: name
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SitePublicationModel> products = sitesService.SearchPublicationsNonOptimized(siteId, page, pageSize, name);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
