namespace Uccs.Fair;

public class SitePolicyChangeModel(SitePolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Change { get; set; } = operation.Operation;
	public Role[] Creators { get; set; } = operation.Creators;
	public ApprovalRequirement Approval { get; set; } = operation.Approval;
}
