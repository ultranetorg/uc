namespace Uccs.Fair;

public static class OperationClassUtils
{
	public static FairOperationClass GetOperationClass(Survey survey)
	{
		return ((StoreApprovalPolicyChange) survey.Options[0].Operation).Operation;
	}
}
