namespace Uccs.Fair;

public class SiteApprovalPolicyChangeModel(StoreApprovalPolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Operation { get; } = operation.Operation;
	public ApprovalRequirement Approval { get; } = operation.Approval;
}
