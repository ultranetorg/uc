namespace Uuc.Models.Accounts;

public class TokenBalance
{
	public string TokenName { get; set; } = null!;

	public string Balance { get; set; } = null!; // TODO: BigInteger
}
