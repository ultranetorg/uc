namespace Uuc.Models;

public abstract class BaseAccount
{
	public string Name { get; set; } = null!;

	public IEnumerable<NetworkBalance>? Balances { get; set; }
}
