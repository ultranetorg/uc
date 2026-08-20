using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsController
(
#if DEBUG
	ILogger<ProductsController> logger,
#endif
	ProductsService productsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProductSearchResultModel> Search([FromQuery] string? query, ProductType productType, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Query}, {Pagination}", nameof(ProductsController), nameof(ProductsController.Search), query, pagination);
#endif

		SearchQueryValidator.Validate(query);
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		return productsService.Search(query, productType, page, pageSize, cancellationToken);
	}

	[HttpGet("{productId}/publications")]
	public IEnumerable<ProductPublicationModel> GetProductPublications(string productId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {ProductId}, {Pagination}", nameof(ProductsController), nameof(GetProductPublications), productId, pagination);
#endif

		AutoIdValidator.Validate(productId, nameof(Product).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		var result = productsService.GetProductPublications(productId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string productId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {ProductId}", nameof(ProductsController), nameof(GetDetails), productId);
#endif

		AutoIdValidator.Validate(productId, nameof(Product).ToLower());

		return productsService.GetDetails(productId);
	}

	[HttpGet("{productId}/stores")]
	public IEnumerable<ProductStoreModel> GetProductStores(string productId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG		
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {ProductId}, {Pagination}", nameof(ProductsController), nameof(GetDetails), productId, pagination);
#endif

		AutoIdValidator.Validate(productId, nameof(Product).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProductStoreModel> productStores = productsService.GetProductStores(productId, page, pageSize, cancellationToken);

		return this.OkPaged(productStores.Items, page, pageSize, productStores.TotalItems);
	}

	//[HttpGet("search")]
	//public IEnumerable<ProductSearchResultBaseModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	//{
	//	logger.LogInformation("GET {ControllerName}.{ActionName} called with {Query}", nameof(ProductsController), nameof(ProductsController.SearchLite), query);

	//	SearchQueryValidator.Validate(query);

	//	return productsService.SearchLite(query, StoreConstants.SearchLitePageSize, cancellationToken);
	//}
}