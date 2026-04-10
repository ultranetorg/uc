using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	UserNameValidator userNameValidator,
	AccountsService accountsService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel Get(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(UsersController), nameof(Get), name);

		userNameValidator.Validate(name);

		return accountsService.Get(name);
	}
}
