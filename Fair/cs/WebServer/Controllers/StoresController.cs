using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoresController
(
#if DEBUG
	ILogger<StoresController> logger,
#endif
	StoresService storesService,
	SearchService searchService
) : BaseController
{
	[HttpGet("{storeId}/policies")]
	public IEnumerable<PolicyModel> GetPolicies(string storeId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(StoresController.GetPolicies), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetPolicies(storeId);
	}

	[HttpGet]
	public IEnumerable<StoreBaseModel> Search([FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Page}", nameof(StoresController), nameof(StoresController.Search), query, page);
#endif

		PaginationValidator.Validate(page);
		StoreSearchQueryValidator.Validate(query);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<StoreBaseModel> result = searchService.SearchStores(query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(result.Items, pageValue, pageSizeValue, result.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<StoreSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}", nameof(StoresController), nameof(StoresController.SearchLite), query);
#endif

		SearchQueryValidator.Validate(query);

		return searchService.SearchStoresLite(query, 0, StoreConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("{storeId}")]
	public StoreModel GetDetails(string storeId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(StoresController.GetDetails), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetDetails(storeId);
	}
}
