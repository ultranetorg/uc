namespace UO.DomainModels.Operations;

public class DomainBidModel : BaseOperationModel
{
	public string Name { get; set; } = null!;

	public long Bid { get; set; }
}
