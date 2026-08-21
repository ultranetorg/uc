using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
#if DEBUG
	ILogger<PublicationsController> logger,
#endif
	PublicationsService publicationsService,
	ProductsService productsService,
	SearchService searchService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string publicationId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}", nameof(PublicationsController), nameof(PublicationsController.GetDetails), publicationId);
#endif

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetDetails(publicationId);
	}

	[HttpGet("{publicationId}/versions")]
	public PublicationVersionInfo GetVersionLatest(string publicationId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}", nameof(PublicationsController), nameof(PublicationsController.GetVersionLatest), publicationId);
#endif

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetVersions(publicationId);
	}

	[HttpGet("{publicationId}/diff")]
	public PublicationDetailsDiffModel GetDiff(string publicationId, [FromQuery(Name = "to")] int version)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}, {Version}", nameof(PublicationsController), nameof(PublicationsController.GetDiff), publicationId, version);
#endif

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		VersionValidator.Validate(publicationId, version);

		return productsService.GetDiff(publicationId, version);
	}

	[HttpGet("~/api/stores/{storeId}/categories/publications")]
	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublications(string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(PublicationsController), nameof(PublicationsController.GetCategoriesPublications), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return publicationsService.GetCategoriesPublicationsNotOptimized(storeId, cancellationToken);
	}

	[HttpGet("~/api/stores/{storeId}/publications")]
	public IEnumerable<PublicationExtendedModel> Search(string storeId, [FromQuery] string? query, [FromQuery] string[]? categoriesIds, [FromQuery] ProductType? type, [FromQuery] int? page, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Query}, {CategoriesIds}, {Type}, {Page}",
			nameof(PublicationsController), nameof(PublicationsController.Search), storeId, query, categoriesIds, type, page);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		SearchQueryValidator.Validate(query);

		CategoriesIdsValidator.Validate(categoriesIds);
		ProductTypeValidator.Validate(type);
		SearchParamsValidator.Validate(categoriesIds, type);

		PaginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page, 20);
		return searchService.SearchPublications(storeId, query, categoriesIds, type, pageValue, pageSizeValue, cancellationToken);
	}

	[HttpGet("~/api/stores/{storeId}/publications/search")]
	public IEnumerable<PublicationBaseModel> SearchLite(string storeId, [FromQuery] string? query, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Query}", nameof(PublicationsController), nameof(PublicationsController.SearchLite), storeId, query);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		SearchQueryValidator.Validate(query);

		return searchService.SearchLitePublications(storeId, query, 0, StoreConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("~/api/categories/{categoryId}/publications")]
	public IEnumerable<PublicationExtendedModel> GetCategoryPublications(string categoryId, [FromQuery] int? page, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {CategoryId}, {Page}", nameof(PublicationsController), nameof(PublicationsController.GetCategoryPublications), categoryId, page);
#endif

		AutoIdValidator.Validate(categoryId, nameof(Category).ToLower());
		PaginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page, CategoriesPublications.DefaultCategoryPageSize);
		TotalItemsResult<PublicationExtendedModel> publications = publicationsService.GetCategoryPublicationsNotOptimized(categoryId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(publications.Items, pageValue, pageSizeValue, publications.TotalItems);
	}
}
