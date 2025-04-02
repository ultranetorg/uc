using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	IEntityIdValidator entityIdValidator,
	IUsersService usersService
) : BaseController
{
	[HttpGet("{userId}")]
	public UserModel Get(string userId)
	{
		logger.LogInformation($"GET {nameof(UsersController)}.{nameof(UsersController.Get)} method called with {{UserId}}", userId);

		entityIdValidator.Validate(userId, nameof(Account).ToLower());

		return usersService.Get(userId);
	}
}
