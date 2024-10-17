namespace UO.DomainModels.Operations;

public abstract class BaseOperationModel
{
	public string Id { get; set; } = null!;

	public string Signer { get; set; } = null!;

	public string TransactionId { get; set; } = null!;
}
