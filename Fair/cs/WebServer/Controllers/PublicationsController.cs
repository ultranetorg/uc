using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	SearchQueryValidator searchQueryValidator,
	PublicationsService publicationsService,
	ProductsService productsService,
	SearchService searchService,
	VersionValidator versionValidator
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetDetails)} method called with {{PublicationId}}", publicationId);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetDetails(publicationId);
	}

	[HttpGet("{publicationId}/versions")]
	public PublicationVersionInfo GetVersionLatest(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetVersionLatest)} method called with {{PublicationId}}", publicationId);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetVersions(publicationId);
	}

	[HttpGet("{publicationId}/diff")]
	public PublicationDetailsDiffModel GetDiff(string publicationId, [FromQuery(Name = "to")] int version)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}, {Version}", nameof(PublicationsController), nameof(GetDiff), publicationId, version);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		versionValidator.Validate(publicationId, version);

		return productsService.GetDiff(publicationId, version);
	}

	[HttpGet("~/api/sites/{siteId}/categories/publications")]
	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublications(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoriesPublications)} method called with {{SiteId}}", siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());

		return publicationsService.GetCategoriesPublicationsNotOptimized(siteId, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/publications")]
	public IEnumerable<PublicationExtendedModel> Search(string siteId, [FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {Query}, {Page}", nameof(PublicationsController), nameof(PublicationsController.Search), siteId, query, page);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		searchQueryValidator.Validate(query);
		paginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		return searchService.SearchPublications(siteId, query, pageValue, pageSizeValue, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/publications/search")]
	public IEnumerable<PublicationBaseModel> SearchLite(string siteId, [FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.SearchLite)} method called with {{SiteId}}, {{Query}}", siteId, query);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		searchQueryValidator.Validate(query);

		return searchService.SearchLitePublications(siteId, query, 0, SiteConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("~/api/categories/{categoryId}/publications")]
	public IEnumerable<PublicationExtendedModel> GetCategoryPublications(string categoryId, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoryPublications)} method called with {{CategoryId}}, {{Page}}", categoryId, page);

		autoIdValidator.Validate(categoryId, nameof(Category).ToLower());
		paginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page, CategoriesPublications.DefaultCategoryPageSize);
		TotalItemsResult<PublicationExtendedModel> publications = publicationsService.GetCategoryPublicationsNotOptimized(categoryId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(publications.Items, pageValue, pageSizeValue, publications.TotalItems);
	}
}
