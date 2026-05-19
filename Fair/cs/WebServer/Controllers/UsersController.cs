using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class UsersController
(
	ILogger<UsersController> logger,
	UserNameValidator userNameValidator,
	IAutoIdValidator autoIdValidator,
	AccountsService accountsService,
	UsersService usersService
) : BaseController
{
	[HttpGet("{name}")]
	public UserModel Get(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(UsersController), nameof(Get), name);

		userNameValidator.Validate(name);

		return accountsService.Get(name);
	}

	[HttpHead("{userId}/sites/{siteId}")]
	public IActionResult SiteExists(string userId, string siteId)
	{
		logger.LogInformation("HEAD {ControllerName}.{MethodName} called with {UserId} and {SiteId}", nameof(UsersController), nameof(SiteExists), userId, siteId);

		autoIdValidator.Validate(userId, nameof(User));
		autoIdValidator.Validate(siteId, nameof(Site));

		return usersService.SiteExists(userId, siteId) ? Ok() : NotFound();
	}
}
