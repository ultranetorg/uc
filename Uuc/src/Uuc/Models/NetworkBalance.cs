namespace Uuc.Models;

public class NetworkBalance
{
	public string NetworkName { get; set; } = null!;

	public IEnumerable<TokenBalance> Balances { get; set; } = null!;
}
