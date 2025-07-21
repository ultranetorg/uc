namespace Uccs.Fair;

public class SitePolicyChangeModel(SitePolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Change { get; set; } = operation.Change;
	public ApprovalPolicy Policy { get; set; } = operation.Policy;
}
