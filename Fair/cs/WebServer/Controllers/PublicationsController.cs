using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	PublicationsService publicationsService,
	ProductsService productsService,
	SearchService searchService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetDetails)} method called with {{PublicationId}}", publicationId);

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetDetails(publicationId);
	}

	[HttpGet("{publicationId}/versions")]
	public PublicationVersionInfo GetVersionLatest(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetVersionLatest)} method called with {{PublicationId}}", publicationId);

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetVersions(publicationId);
	}

	[HttpGet("{publicationId}/diff")]
	public PublicationDetailsDiffModel GetDiff(string publicationId, [FromQuery(Name = "to")] int version)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}, {Version}", nameof(PublicationsController), nameof(GetDiff), publicationId, version);

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		VersionValidator.Validate(publicationId, version);

		return productsService.GetDiff(publicationId, version);
	}

	[HttpGet("~/api/stores/{storeId}/categories/publications")]
	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublications(string storeId, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoriesPublications)} method called with {{StoreId}}", storeId);

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return publicationsService.GetCategoriesPublicationsNotOptimized(storeId, cancellationToken);
	}

	[HttpGet("~/api/stores/{storeId}/publications")]
	public IEnumerable<PublicationExtendedModel> Search(string storeId, [FromQuery] string? query, [FromQuery] string[]? categoriesIds, [FromQuery] ProductType? type, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Query}, {CategoriesIds}, {Type}, {Page}",
			nameof(PublicationsController), nameof(PublicationsController.Search), storeId, query, categoriesIds, type, page);

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
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.SearchLite)} method called with {{StoreId}}, {{Query}}", storeId, query);

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		SearchQueryValidator.Validate(query);

		return searchService.SearchLitePublications(storeId, query, 0, StoreConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("~/api/categories/{categoryId}/publications")]
	public IEnumerable<PublicationExtendedModel> GetCategoryPublications(string categoryId, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoryPublications)} method called with {{CategoryId}}, {{Page}}", categoryId, page);

		AutoIdValidator.Validate(categoryId, nameof(Category).ToLower());
		PaginationValidator.Validate(page);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page, CategoriesPublications.DefaultCategoryPageSize);
		TotalItemsResult<PublicationExtendedModel> publications = publicationsService.GetCategoryPublicationsNotOptimized(categoryId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(publications.Items, pageValue, pageSizeValue, publications.TotalItems);
	}
}
