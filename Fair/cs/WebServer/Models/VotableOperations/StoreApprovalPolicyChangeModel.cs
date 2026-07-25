namespace Uccs.Fair;

public class StoreApprovalPolicyChangeModel(StoreApprovalPolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Operation { get; } = operation.Operation;
	public ApprovalRequirement Approval { get; } = operation.Approval;
}
