using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	IAutoIdValidator autoIdValidator,
	IUsersService usersService
) : BaseController
{
	[HttpGet("{userId}")]
	public UserModel Get(string userId)
	{
		logger.LogInformation($"GET {nameof(UsersController)}.{nameof(UsersController.Get)} method called with {{UserId}}", userId);

		autoIdValidator.Validate(userId, nameof(Account).ToLower());

		return usersService.Get(userId);
	}
}
