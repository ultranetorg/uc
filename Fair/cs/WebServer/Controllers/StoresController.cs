using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoresController
(
	ILogger<StoresController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	StoreSearchQueryValidator storeSearchQueryValidator,
	SearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator,
	StoresService storesService,
	UsersService usersService,
	SearchService searchService
) : BaseController
{
	[HttpGet("default")]
	public IEnumerable<StoreBaseModel> Default(CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called without parameters", nameof(StoresController), nameof(StoresController.Default));

		return storesService.GetDefaultStores(cancellationToken);
	}

	[HttpGet("{storeId}/users")]
	public IEnumerable<UserModel> GetUsers(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Pagination}", nameof(StoresController), nameof(GetUsers), storeId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store));
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserModel> result = usersService.GetStoreUsers(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("{storeId}/users/search")]
	public IEnumerable<UserModel> SearchStoreUsers(string storeId, [FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Query}, {Limit}", nameof(StoresController), nameof(SearchStoreUsers), storeId, query, limit);

		autoIdValidator.Validate(storeId, nameof(Store));
		storeSearchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchStoreUsers(storeId, query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[HttpGet("{storeId}/publishers")]
	public IEnumerable<PublisherModel> GetPublishers(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Search}, {Pagination}", nameof(StoresController), nameof(GetPublishers), storeId, search, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherModel> publishers = storesService.GetPublishers(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(publishers.Items, page, pageSize, publishers.TotalItems);
	}

	[HttpGet("{storeId}/moderators")]
	public IEnumerable<ModeratorModel> GetModerators(string storeId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(GetModerators), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetModerators(storeId, cancellationToken);
	}

	[HttpGet("{storeId}/policies")]
	public IEnumerable<PolicyModel> GetPolicies(string storeId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(GetPolicies), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetPolicies(storeId);
	}

	[HttpGet]
	public IEnumerable<StoreBaseModel> Search([FromQuery] string? query, [FromQuery] int? page, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Page}", nameof(StoresController), nameof(Search), query, page);

		paginationValidator.Validate(page);
		storeSearchQueryValidator.Validate(query);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(page);
		TotalItemsResult<StoreBaseModel> result = searchService.SearchStores(query, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(result.Items, pageValue, pageSizeValue, result.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<StoreSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}", nameof(StoresController), nameof(SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteStores(query, 0, StoreConstants.SearchLitePageSize, cancellationToken);
	}

	[HttpGet("{storeId}")]
	public StoreModel GetDetails(string storeId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoresController), nameof(GetDetails), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return storesService.GetDetails(storeId);
	}
}
