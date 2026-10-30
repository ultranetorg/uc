using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/users")]
public class UsersController
(
#if DEBUG
	ILogger<UsersController> logger,
#endif
	StoreUsersService storeUsersService
) : BaseController
{
	[HttpGet]
	public IEnumerable<UserModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Pagination}", nameof(UsersController), nameof(UsersController.GetAll), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store));
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserModel> result = storeUsersService.GetStoreUsers(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("search")]
	public IEnumerable<UserModel> Search(string storeId, [FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {StoreId}, {Query}, {Limit}", nameof(UsersController), nameof(UsersController.Search), storeId, query, limit);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store));
		StoreSearchQueryValidator.Validate(query);
		LimitValidator.Validate(limit);

		return storeUsersService.SearchStoreUsers(storeId, query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}
}
