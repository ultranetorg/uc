using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsController
(
	ILogger<AccountsController> logger,
	AccountsService accountsService,
	ISearchService searchService,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator,
	UserNameValidator userNameValidator
) : BaseController
{
	//[HttpGet("address/{address}")]
	//public AccountModel GetByAddress(string address)
	//{
	//	logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountAddress}", nameof(AccountsController), nameof(GetByAddress), address);

	//	accountAddressValidator.Validate(address);

	//	return accountsService.GetByAddress(address);
	//}

	[HttpGet("/api/users/{name}")]
	public AccountBaseModel Get(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(AccountsController), nameof(Get), name);

		userNameValidator.Validate(name);

		return accountsService.Get(name);
	}

	[HttpGet("/api/users/{name}/details")]
	public AccountModel GetDetails(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(AccountsController), nameof(GetDetails), name);

		userNameValidator.Validate(name);

		return accountsService.GetDetails(name);
	}

	[HttpGet("{accountId}")]
	public UserModel GetById(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(GetById), accountId);

		autoIdValidator.Validate(accountId, nameof(Net.User).ToLower());

		return accountsService.GetById(accountId);
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

	[Obsolete("This endpoint is deprected use GetUserAvatar instead")]
	[HttpGet("{accountId}/avatar")]
	public FileContentResult GetAvatar(string accountId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AccountId}", nameof(AccountsController), nameof(GetAvatar), accountId);

		autoIdValidator.Validate(accountId, nameof(Net.User).ToLower());

		return accountsService.GetAvatar(accountId);
	}

	[HttpGet("/api/users/{name}/avatar")]
	public FileContentResult GetUserAvatar(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Name}", nameof(AccountsController), nameof(GetUserAvatar), name);

		userNameValidator.Validate(name);

		return accountsService.GetUserAvatar(name);
	}
}
