using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	IAccountsService usersService,
	ISearchService searchService,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator
) : BaseController
{
	[HttpGet("{accountId}")]
	public UserModel Get(string accountId)
	{
		logger.LogInformation($"GET {nameof(AccountsController)}.{nameof(AccountsController.Get)} method called with {{AccountId}}", accountId);

		autoIdValidator.Validate(accountId, nameof(Account).ToLower());

		return usersService.Get(accountId);
	}

	[HttpGet("search")]
	public IEnumerable<AccountSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(AccountsController)}.{nameof(AccountsController.SearchLite)} method called with {{Query}}", query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteAccounts(query, SearchConstants.SearchAccountsLimit, cancellationToken);
	}
}
