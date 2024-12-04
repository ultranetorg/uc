using Uccs.Net;

namespace Uuc.Models.Accounts;

public class Account : BaseAccount
{
	public AccountKey AccountKey { get; init; } = null!;

	public string Address => AccountKey.GetPublicAddress();
}
