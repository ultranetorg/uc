using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	ReviewsService reviewsService,
	SearchService searchService,
	UsersService usersService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel GetUser(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(GetUser), name);

		UserNameValidator.Validate(name);

		return usersService.GetUserByName(name);
	}

	[HttpGet("{userId}/authors")]
	public UserAuthorsModel GetUserAuthors(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}", nameof(UsersController), nameof(GetUserAuthors), userId);

		AutoIdValidator.Validate(userId, nameof(User));

		return usersService.GetUserAuthors(userId);
	}

	[HttpGet("{name}/details")]
	public UserDetailsModel GetDetails(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(GetDetails), name);

		UserNameValidator.Validate(name);

		return usersService.GetDetails(name);
	}

	[HttpHead("{userId}/stores/{storeId}")]
	public IActionResult StoreExists(string userId, string storeId)
	{
		logger.LogInformation("HEAD {ControllerName}.{ActionName} called with {UserId}, {StoreId}", nameof(UsersController), nameof(StoreExists), userId, storeId);

		AutoIdValidator.Validate(userId, nameof(User));
		AutoIdValidator.Validate(storeId, nameof(Store));

		return usersService.StoreExists(userId, storeId) ? Ok() : NotFound();
	}

	[HttpGet("{userId}/reviews")]
	public IEnumerable<ReviewModel> GetReviews(string userId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}, {Pagination}", nameof(UsersController), nameof(GetReviews), userId, pagination);

		AutoIdValidator.Validate(userId, nameof(User));
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> result = reviewsService.GetUserReviewsNotOptimized(userId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("by-id/{userId}/avatar")]
	public FileContentResult GetAvatarById(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {UserId}", nameof(UsersController), nameof(GetAvatarById), userId);

		AutoIdValidator.Validate(userId, nameof(User));

		return usersService.GetAvatarById(userId);
	}

	[HttpGet("by-name/{name}/avatar")]
	public FileContentResult GetAvatarByName(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Name}", nameof(UsersController), nameof(GetAvatarByName), name);

		UserNameValidator.Validate(name);

		return usersService.GetAvatarByName(name);
	}

	[HttpGet]
	public IEnumerable<UserBaseAvatarModel> Search([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Limit}", nameof(UsersController), nameof(Search), query, limit);

		SearchQueryValidator.Validate(query);
		LimitValidator.Validate(limit);

		return searchService.SearchUser(query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}
}