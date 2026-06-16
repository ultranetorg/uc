namespace Uccs.Fair;

public class PolicyModel(Policy policy)
{
	public FairOperationClass OperationClass { get; } = policy.OperationClass;
	public IEnumerable<Role> Creators { get; } = policy.Creators.GetFlags();
	public ApprovalRequirement Approval { get; } = policy.Approval;
}
