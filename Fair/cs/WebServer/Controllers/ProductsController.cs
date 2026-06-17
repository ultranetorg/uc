using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ProductsService productsService
) : BaseController
{
	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string productId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {ProductId}", nameof(ProductsController), nameof(GetDetails), productId);

		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return productsService.GetDetails(productId);
	}

	[HttpGet("{productId}/stores")]
	public IEnumerable<ProductStoreModel> GetProductStores(string productId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {ProductId}, {Pagination}", nameof(ProductsController), nameof(GetDetails), productId, pagination);

		autoIdValidator.Validate(productId, nameof(Product).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProductStoreModel> productStores = productsService.GetProductStores(productId, page, pageSize, cancellationToken);

		return this.OkPaged(productStores.Items, page, pageSize, productStores.TotalItems);

	}
}