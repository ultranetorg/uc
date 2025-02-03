using Microsoft.AspNetCore.Mvc;

namespace Uccs.Smp;

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

		UserModel user = usersService.Find(userId);
		if (user == null)
		{
			throw new EntityNotFoundException(nameof(Account).ToLower(), userId);
		}

		return user;
	}
}
