namespace Uuc.Models.Accounts;

public class NetworkBalance
{
	public string NetworkName { get; set; } = null!;

	public IEnumerable<TokenBalance> Balances { get; set; } = null!;
}
