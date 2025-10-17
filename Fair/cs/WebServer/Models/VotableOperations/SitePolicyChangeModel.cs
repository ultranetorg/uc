namespace Uccs.Fair;

public class SitePolicyChangeModel(SitePolicyChange operation) : BaseVotableOperationModel(operation)
{
	public FairOperationClass Change { get; set; } = operation.Operation;
	public ApprovalRequirement Approval { get; set; } = operation.Approval;
}
