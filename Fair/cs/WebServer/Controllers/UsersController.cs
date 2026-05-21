using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	UserNameValidator userNameValidator,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	UsersService usersService,
	ReviewsService reviewsService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel GetUser(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(UsersController), nameof(GetUser), name);

		userNameValidator.Validate(name);

		return usersService.GetUserByName(name);
	}

	[HttpGet("{userId}/authors")]
	public UserAuthorsModel GetUserAuthors(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {UserId}", nameof(UsersController), nameof(GetUserAuthors), userId);

		autoIdValidator.Validate(userId, nameof(User));

		return usersService.GetUserAuthors(userId);
	}

	[HttpGet("{name}/details")]
	public UserDetailsModel GetDetails(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(UsersController), nameof(GetDetails), name);

		userNameValidator.Validate(name);

		return usersService.GetDetails(name);
	}

	[HttpHead("{userId}/sites/{siteId}")]
	public IActionResult SiteExists(string userId, string siteId)
	{
		logger.LogInformation("HEAD {ControllerName}.{MethodName} called with {UserId} and {SiteId}", nameof(UsersController), nameof(SiteExists), userId, siteId);

		autoIdValidator.Validate(userId, nameof(User));
		autoIdValidator.Validate(siteId, nameof(Site));

		return usersService.SiteExists(userId, siteId) ? Ok() : NotFound();
	}

	[HttpGet("{userId}/reviews")]
	public IEnumerable<ReviewModel> GetReviews(string userId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {UserId}, {Pagination}", nameof(UsersController), nameof(GetReviews), userId, pagination);

		autoIdValidator.Validate(userId, nameof(User));
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> result = reviewsService.GetUserReviewsNotOptimized(userId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("{userId}/avatar")]
	public FileContentResult GetAvatar(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {UserId}", nameof(UsersController), nameof(GetAvatar), userId);

		autoIdValidator.Validate(userId, nameof(User));

		return usersService.GetAvatar(userId);
	}

	//[HttpGet("by-id/{userId}")]
	//public UserModel GetUserById(string userId)
	//{
	//	logger.LogInformation("GET {ControllerName}.{MethodName} called with {UserId}", nameof(UsersController), nameof(GetUserById), userId);

	//	autoIdValidator.Validate(userId, nameof(User));

	//	return usersService.GetUserById(userId);
	//}
}