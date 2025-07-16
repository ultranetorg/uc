namespace Uccs.Fair;

public class SitePolicyChangeModel(SitePolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Change { get; set; } = operation.Change;
	public ChangePolicy Policy { get; set; } = operation.Policy;
}
