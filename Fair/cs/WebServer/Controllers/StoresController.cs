using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoresController
(
#if DEBUG
	ILogger<StoresController> logger,
#endif
	StoresService storesService,
	UsersService usersService,
	SearchService searchService
) : BaseController
{
	[HttpGet("default")]
	public IEnumerable<StoreBaseModel> Default(CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called without parameters", nameof(StoresController), nameof(StoresController.Default));
#endif

		return storesService.GetDefaultStores(cancellationToken);
	}

	[HttpGet("{storeId}/users")]
	public IEnumerable<UserModel> GetUsers(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Pagination}", nameof(StoresController), nameof(StoresController.GetUsers), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store));
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserModel> result = usersService.GetStoreUsers(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("{storeId}/users/search")]
	public IEnumerable<UserModel> SearchStoreUsers(string storeId, [FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Query}, {Limit}", nameof(StoresController), nameof(StoresController.SearchStoreUsers), storeId, query, limit);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store));
		StoreSearchQueryValidator.Validate(query);
		LimitValidator.Validate(limit);

		return searchService.SearchStoreUsers(storeId, query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[HttpGet("{storeId}/publishers")]
	public IEnumerable<PublisherModel> GetPublishers(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Search}, {Pagination}", nameof(StoresController), nameof(StoresController.GetPublishers), storeId, search, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherModel> publishers = storesService.GetPublishers(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(publishers.Items, page, pageSize, publishers.TotalItems);
	}

	[HttpGet("{storeId}/moderators")]
	public IEnumerable<ModeratorModel> GetModerators(string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(StoresController.GetModerators), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetModerators(storeId, cancellationToken);
	}

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

		return searchService.SearchLiteStores(query, 0, StoreConstants.SearchLitePageSize, cancellationToken);
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
