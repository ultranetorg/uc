using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersController
(
#if DEBUG
	ILogger<UsersController> logger,
#endif
	ReviewsService reviewsService,
	SearchService searchService,
	UsersService usersService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel Get(string name)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(UsersController.Get), name);
#endif

		UserNameValidator.Validate(name);

		return usersService.GetByName(name);
	}

	[HttpGet("{userId}/authors")]
	public UserAuthorsModel GetAuthors(string userId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}", nameof(UsersController), nameof(UsersController.GetAuthors), userId);
#endif

		AutoIdValidator.Validate(userId, nameof(User));

		return usersService.GetAuthors(userId);
	}

	[HttpGet("{name}/details")]
	public UserDetailsModel GetDetails(string name)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(UsersController.GetDetails), name);
#endif

		UserNameValidator.Validate(name);

		return usersService.GetDetails(name);
	}

	[HttpHead("{userId}/stores/{storeId}")]
	public IActionResult StoreExists(string userId, string storeId)
	{
#if DEBUG
		logger.LogInformation("HEAD {ControllerName}.{ActionName} called with {UserId}, {StoreId}", nameof(UsersController), nameof(UsersController.StoreExists), userId, storeId);
#endif

		AutoIdValidator.Validate(userId, nameof(User));
		AutoIdValidator.Validate(storeId, nameof(Store));

		return usersService.StoreExists(userId, storeId) ? Ok() : NotFound();
	}

	[HttpGet("{userId}/reviews")]
	public IEnumerable<ReviewModel> GetReviews(string userId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}, {Pagination}", nameof(UsersController), nameof(UsersController.GetReviews), userId, pagination);
#endif

		AutoIdValidator.Validate(userId, nameof(User));
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> result = reviewsService.GetUserReviewsNotOptimized(userId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("by-id/{userId}/avatar")]
	public FileContentResult GetAvatarById(string userId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {UserId}", nameof(UsersController), nameof(UsersController.GetAvatarById), userId);
#endif

		AutoIdValidator.Validate(userId, nameof(User));

		return usersService.GetAvatarById(userId);
	}

	[HttpGet("by-name/{name}/avatar")]
	public FileContentResult GetAvatarByName(string name)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Name}", nameof(UsersController), nameof(UsersController.GetAvatarByName), name);
#endif

		UserNameValidator.Validate(name);

		return usersService.GetAvatarByName(name);
	}

	[HttpGet]
	public IEnumerable<UserBaseAvatarModel> Search([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Limit}", nameof(UsersController), nameof(UsersController.Search), query, limit);
#endif

		SearchQueryValidator.Validate(query);
		LimitValidator.Validate(limit);

		return searchService.SearchUsers(query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}
}