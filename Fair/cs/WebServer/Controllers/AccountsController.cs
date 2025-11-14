using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	AccountsService accountsService,
	ISearchService searchService,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator
) : BaseController
{
	[HttpGet("{accountId}")]
	public UserModel Get(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(Get), accountId);

		autoIdValidator.Validate(accountId, nameof(Account).ToLower());

		return accountsService.Get(accountId);
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
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}", nameof(AccountsController), nameof(SearchLite), query);

		searchQueryValidator.Validate(query);

		return searchService.SearchLiteAccounts(query, SearchConstants.SearchAccountsLimit, cancellationToken);
	}

	[HttpGet("{accountId}/avatar")]
	public FileContentResult GetAvatar([FromQuery] string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(GetAvatar), accountId);

		autoIdValidator.Validate(accountId, nameof(Account).ToLower());

		return accountsService.GetAvatar(accountId);
	}
}
