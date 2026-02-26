namespace Uccs.Fair;

public class PolicyModel(Policy policy)
{
	public FairOperationClass OperationClass { get; set; } = policy.OperationClass;
	public ApprovalRequirement Approval { get; set; } = policy.Approval;
}
