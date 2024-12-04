namespace Uuc.Models.Accounts;

public abstract class BaseAccount
{
	public string Name { get; set; } = null!;

	public IEnumerable<NetworkBalance>? Balances { get; set; }

	public IEnumerable<Operation>? Operations { get; set; }
}
