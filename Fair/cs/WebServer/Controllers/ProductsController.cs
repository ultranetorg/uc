using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	SearchQueryValidator searchQueryValidator,
	ProductsService productsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProductSearchResultModel> Search([FromQuery] string? query, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Query}, {Pagination}", nameof(ProductsController), nameof(ProductsController.Search), query, pagination);

		searchQueryValidator.Validate(query);
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		return productsService.Search(query, page, pageSize, cancellationToken);
	}

	[HttpGet("search")]
	public IEnumerable<ProductSearchResultBaseModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Query}", nameof(ProductsController), nameof(ProductsController.SearchLite), query);

		searchQueryValidator.Validate(query);

		return productsService.SearchLite(query, StoreConstants.SearchLitePageSize, cancellationToken);
	}

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