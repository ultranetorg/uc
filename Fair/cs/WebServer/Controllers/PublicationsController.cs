using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ISearchQueryValidator searchQueryValidator,
	IPublicationsService publicationsService,
	ISearchService searchService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetPublication(publicationId);
	}

	[HttpGet("{publicationId}/versions")]
	public PublicationVersionInfo GetVersionLatest(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetVersionLatest)} method called with {{PublicationId}}", publicationId);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetVersions(publicationId);
	}

	[HttpGet("~/api/sites/{siteId}/categories/publications")]
	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublications(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoriesPublications)} method called with {{SiteId}}", siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return publicationsService.GetCategoriesPublicationsNotOptimized(siteId, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/publications")]
	public IEnumerable<PublicationExtendedModel> Search(string siteId, [FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Search)} method called with {{SiteId}}, {{Query}}, {{Page}}", siteId, query, page);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		searchQueryValidator.Validate(query);
		paginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<PublicationExtendedModel> products = searchService.SearchPublications(siteId, query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}

	[HttpGet("~/api/sites/{siteId}/publications/search")]
	public IEnumerable<PublicationBaseModel> SearchLite(string siteId, [FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.SearchLite)} method called with {{SiteId}}, {{Query}}", siteId, query);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		searchQueryValidator.Validate(query);

		return searchService.SearchLitePublications(siteId, query, 0, SiteConstants.SEARCH_LITE_PAGE_SIZE, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/authors/{authorId}/publications")]
	public IEnumerable<PublicationAuthorModel> GetAuthorPublications(string siteId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{SiteId}}, {{AuthorId}}, {{Pagination}}", siteId, authorId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationAuthorModel> products = publicationsService.GetAuthorPublicationsNotOptimized(siteId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}

	[HttpGet("~/api/categories/{categoryId}/publications")]
	public IEnumerable<PublicationModel> GetCategoryPublications(string categoryId, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoryPublications)} method called with {{CategoryId}}, {{Page}}", categoryId, page);

		autoIdValidator.Validate(categoryId, nameof(Category).ToLower());
		paginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<PublicationModel> publications = publicationsService.GetCategoryPublicationsNotOptimized(categoryId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(publications.Items, pageValue, pageSizeValue, publications.TotalItems);
	}
}
