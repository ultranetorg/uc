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
	[HttpGet("{unpublishedProductId}")]
	public UnpublishedProductDetailsModel Get(string siteId, string unpublishedProductId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {UnpublishedPublicationId}", nameof(ModeratorUnpublishedProductsController), nameof(Get), siteId, unpublishedProductId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(unpublishedProductId, nameof(EntityNames.UnpublishedProductEntityName).ToLower());

		return productsService.GetUnpublishedProduct(siteId, unpublishedProductId);
	}

	[HttpGet]
	public IEnumerable<UnpublishedProductModel> GetUnpublishedPublications(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {Pagination}", nameof(ModeratorUnpublishedProductsController), nameof(GetUnpublishedPublications), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UnpublishedProductModel> products = productsService.GetUnpublishedProducts(siteId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}