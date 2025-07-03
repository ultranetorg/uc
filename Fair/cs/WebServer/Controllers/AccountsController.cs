using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	IAutoIdValidator autoIdValidator,
	IAccountsService usersService
) : BaseController
{
	[HttpGet("{accountId}")]
	public UserModel Get(string accountId)
	{
		logger.LogInformation($"GET {nameof(AccountsController)}.{nameof(AccountsController.Get)} method called with {{AccountId}}", accountId);

		autoIdValidator.Validate(accountId, nameof(Account).ToLower());

		return usersService.Get(accountId);
	}
}
