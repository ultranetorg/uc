namespace Uccs.Fair;

public class SitePolicyChangeModel(SitePolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Change { get; set; } = operation.Change;
	public Role[] Creators { get; set; } = operation.Creators;
	public ApprovalPolicy Approval { get; set; } = operation.Approval;
}
