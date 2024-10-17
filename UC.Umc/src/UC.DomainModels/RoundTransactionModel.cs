namespace UC.DomainModels;

public class RoundTransactionModel
{
	public string Id { get; set; } = null!;

	public string Signer { get; set; } = null!;

	public int OperationsCount { get; set; }
}
