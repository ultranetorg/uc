namespace Uuc.Models.Accounts;

public class Operation
{
	public string Id { get; set; } = null!;

	public string NetworkId { get; set; } = null!;

	public string TransactionId { get; set; } = null!;

	public string SignerAddress { get; set; } = null!;
}
