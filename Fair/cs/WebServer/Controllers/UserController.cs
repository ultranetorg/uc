using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class UserController
(
	ILogger<UserController> logger,
	UserService accountsService,
	SearchService searchService,
	AutoIdValidator autoIdValidator,
	SearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator
) : BaseController
{
	[HttpGet]
	public IEnumerable<UserBaseAvatarModel> Search([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Limit}", nameof(UserController), nameof(Search), query, limit);

		searchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchUser(query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[HttpGet("search")]
	public IEnumerable<UserSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}", nameof(UserController), nameof(SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteUsers(query, SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[Obsolete("This endpoint is deprecated use GetUserAvatar instead")]
	[HttpGet("{accountId}/avatar")]
	public FileContentResult GetAvatar(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {AccountId}", nameof(UserController), nameof(GetAvatar), accountId);

		autoIdValidator.Validate(accountId, nameof(Net.User).ToLower());

		return accountsService.GetAvatar(accountId);
	}
}
