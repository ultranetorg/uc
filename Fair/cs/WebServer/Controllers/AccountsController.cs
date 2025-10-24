using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	IAccountsService usersService,
	ISearchService searchService,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator
) : BaseController
{
	[HttpGet("{accountId}")]
	public UserModel Get(string accountId)
	{
		logger.LogInformation($"GET {nameof(AccountsController)}.{nameof(AccountsController.Get)} method called with {{AccountId}}", accountId);

		autoIdValidator.Validate(accountId, nameof(Account).ToLower());

		return usersService.Get(accountId);
	}

	[HttpGet]
	public IEnumerable<AccountBaseModel> Search([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}, {Limit}", nameof(AccountsController), nameof(Search), query, limit);

		searchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchAccount(query, limit ?? SearchConstants.SearchAccountsLimit, cancellationToken);
	}

	[HttpGet("search")]
	public IEnumerable<AccountSearchLiteModel> SearchLite([FromQuery] string? query, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}", nameof(AccountsController), nameof(AccountsController.SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteAccounts(query, SearchConstants.SearchAccountsLimit, cancellationToken);
	}
}
