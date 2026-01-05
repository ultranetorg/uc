using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	AccountsService accountsService,
	ISearchService searchService,
	AccountAddressValidator accountAddressValidator,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator
) : BaseController
{
	[HttpGet("address/{address}")]
	public AccountModel GetByAddress(string address)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountAddress}", nameof(AccountsController), nameof(GetByAddress), address);

		accountAddressValidator.Validate(address);

		return accountsService.GetByAddress(address);
	}

	[HttpGet("{accountId}")]
	public UserModel Get(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(Get), accountId);

		autoIdValidator.Validate(accountId, nameof(Net.User).ToLower());

		return accountsService.Get(accountId);
	}

	[HttpGet]
	public IEnumerable<AccountBaseAvatarModel> Search([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
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
	public FileContentResult GetAvatar(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(GetAvatar), accountId);

		autoIdValidator.Validate(accountId, nameof(Net.User).ToLower());

		return accountsService.GetAvatar(accountId);
	}
}
