using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsService
(
	ILogger<AccountsService> logger,
	FairMcv mcv
)
{
	[Obsolete("This method is deprected use GetUserAvatar instead")]
	public FileContentResult GetAvatar([NotNull][NotEmpty] string accountId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {AccountId}", nameof(AccountsService), nameof(GetAvatar), accountId);

		Guard.Against.NullOrEmpty(accountId);

		AutoId id = AutoId.Parse(accountId);

		FairUser account = (FairUser) mcv.Users.Latest(id);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), accountId);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}

	private class LoadProductsResult
	{
		public IEnumerable<UserProductModel> ProductsModels { get; set; }

		public Product[] Products { get; set; }
	}
}
