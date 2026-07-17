using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	UserNameValidator userNameValidator,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	UsersService usersService,
	ReviewsService reviewsService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel GetUser(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(GetUser), name);

		userNameValidator.Validate(name);

		return usersService.GetUserByName(name);
	}

	[HttpGet("{userId}/authors")]
	public UserAuthorsModel GetUserAuthors(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}", nameof(UsersController), nameof(GetUserAuthors), userId);

		autoIdValidator.Validate(userId, nameof(User));

		return usersService.GetUserAuthors(userId);
	}

	[HttpGet("{name}/details")]
	public UserDetailsModel GetDetails(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {Name}", nameof(UsersController), nameof(GetDetails), name);

		userNameValidator.Validate(name);

		return usersService.GetDetails(name);
	}

	[HttpHead("{userId}/sites/{siteId}")]
	public IActionResult SiteExists(string userId, string siteId)
	{
		logger.LogInformation("HEAD {ControllerName}.{ActionName} called with {UserId}, {SiteId}", nameof(UsersController), nameof(SiteExists), userId, siteId);

		autoIdValidator.Validate(userId, nameof(User));
		autoIdValidator.Validate(siteId, nameof(Store));

		return usersService.SiteExists(userId, siteId) ? Ok() : NotFound();
	}

	[HttpGet("{userId}/reviews")]
	public IEnumerable<ReviewModel> GetReviews(string userId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} called with {UserId}, {Pagination}", nameof(UsersController), nameof(GetReviews), userId, pagination);

		autoIdValidator.Validate(userId, nameof(User));
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> result = reviewsService.GetUserReviewsNotOptimized(userId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	[HttpGet("by-id/{userId}/avatar")]
	public FileContentResult GetAvatarById(string userId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {UserId}", nameof(UsersController), nameof(GetAvatarById), userId);

		autoIdValidator.Validate(userId, nameof(User));

		return usersService.GetAvatarById(userId);
	}

	[HttpGet("by-name/{name}/avatar")]
	public FileContentResult GetAvatarByName(string name)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Name}", nameof(UsersController), nameof(GetAvatarByName), name);

		userNameValidator.Validate(name);

		return usersService.GetAvatarByName(name);
	}
}