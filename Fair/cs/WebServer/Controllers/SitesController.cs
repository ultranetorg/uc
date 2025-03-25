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
	public IEnumerable<SiteBaseModel> Search([FromQuery] PaginationRequest pagination, [FromQuery] string? title)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.Get)} method called with {{Pagination}}, {{Title}}", pagination, title);

		// TODO: validate search string: title
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SiteBaseModel> sites = sitesService.SearchNonOptimized(page, pageSize, title);

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

	[HttpGet("{siteId}/authors")]
	public IEnumerable<AuthorBaseModel> GetAuthors(string siteId, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetAuthors)} method called with {{SiteId}}, {{Pagination}}", siteId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<AuthorBaseModel> authors = sitesService.GetAuthors(siteId, page, pageSize);
		if (authors == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return this.OkPaged(authors.Items, page, pageSize, authors.TotalItems);
	}

	[HttpGet("{siteId}/categories")]
	public IEnumerable<CategoryParentBaseModel> GetCategories(string siteId, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetCategories)} method called with {{SiteId}}, {{Pagination}}", siteId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<CategoryParentBaseModel> categories = sitesService.GetCategories(siteId, page, pageSize);
		if (categories == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return this.OkPaged(categories.Items, page, pageSize, categories.TotalItems);
	}

	[HttpGet("{siteId}/authors/{authorId}")]
	public SiteAuthorModel GetAuthor(string siteId, string authorId)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetAuthor)} method called with {{SiteId}}, {{AuthorId}}", siteId, authorId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(authorId, nameof(Author).ToLower());

		SiteAuthorModel siteAuthor = sitesService.FindAuthorNonOptimized(siteId, authorId);
		if (siteAuthor == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return siteAuthor;
	}
}
