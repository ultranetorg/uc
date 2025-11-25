using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/products/unpublished")]
public class ModeratorUnpublishedProductsController
(
	ILogger<ModeratorUnpublishedProductsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ProductsService productsService
) : BaseController
{
	[HttpGet("/api/products/unpublished/{unpublishedProductId}")]
	public UnpublishedProductDetailsModel GetUnpublishedProduct(string unpublishedProductId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {UnpublishedPublicationId}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedProduct), unpublishedProductId);

		autoIdValidator.Validate(unpublishedProductId, nameof(EntityNames.UnpublishedProductEntityName).ToLower());

		return productsService.GetUnpublishedProduct(unpublishedProductId);
	}

	[HttpGet("{unpublishedProductId}")]
	public UnpublishedProductDetailsModel GetUnpublishedSiteProduct(string siteId, string unpublishedProductId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {UnpublishedPublicationId}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedSiteProduct), siteId, unpublishedProductId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(unpublishedProductId, nameof(EntityNames.UnpublishedProductEntityName).ToLower());

		return productsService.GetUnpublishedProduct(unpublishedProductId, siteId);
	}

	[HttpGet]
	public IEnumerable<UnpublishedProductModel> GetUnpublishedSitePublications(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {Pagination}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedSitePublications), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UnpublishedProductModel> products = productsService.GetUnpublishedProducts(siteId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}