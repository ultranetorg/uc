namespace UO.DomainModels.Operations;

public class UnitTransferModel : BaseOperationModel
{
	public string To { get; set; } = null!;

	public long BYAmount { get; set; }
	public long ECAmount { get; set; }
	public int ECExpiration { get; set; }
}
