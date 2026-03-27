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
	// TODO: should be moved to ProductsController.
	[HttpHead("/api/products/unpublished/{unpublishedProductId}")]
	public IActionResult HeadUnpublishedProduct(string unpublishedProductId)
	{
		logger.LogInformation("HEAD {ControllerName}.{MethodName} called with {UnpublishedPublicationId}", nameof(ModeratorUnpublishedProductsController), nameof(HeadUnpublishedProduct), unpublishedProductId);

		autoIdValidator.Validate(unpublishedProductId, EntityNames.UnpublishedProductEntityName);

		bool exists = productsService.UnpublishedProductExists(unpublishedProductId);

		return exists ? Ok() : NotFound();
	}

	// TODO: should be moved to ProductsController.
	[HttpGet("/api/products/unpublished/{unpublishedProductId}")]
	public UnpublishedProductDetailsModel GetUnpublishedProduct(string unpublishedProductId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {UnpublishedPublicationId}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedProduct), unpublishedProductId);

		autoIdValidator.Validate(unpublishedProductId, EntityNames.UnpublishedProductEntityName.ToLower());

		return productsService.GetUnpublishedProduct(unpublishedProductId);
	}

	/// <summary>
	/// Returns Products that have not been published on the Site.
	/// </summary>
	[HttpGet("{productId}")]
	public UnpublishedProductDetailsModel GetUnpublishedSiteProduct(string siteId, string productId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {ProductId}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedSiteProduct), siteId, productId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return productsService.GetUnpublishedSiteProduct(siteId, productId);
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