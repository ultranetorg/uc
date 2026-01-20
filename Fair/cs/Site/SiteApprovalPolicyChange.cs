namespace Uccs.Fair;

public class SiteApprovalPolicyChange : SiteOperation
{
	public FairOperationClass	Operation { get; set; }
	//public Role					Creators { get; set; }
	public ApprovalRequirement	Approval { get; set; }

	public override bool		IsValid(McvNet net) =>	Enum.IsDefined<FairOperationClass>(Operation) && 
														//Enum.IsDefined<Role>(Creators)&&
														Enum.IsDefined<ApprovalRequirement>(Approval);

	public override string		Explanation => $"Site={Site}, Operation+{Operation}, Approval={Approval}";
	
	public override void Read(BinaryReader reader)
	{
		Operation	= reader.Read<FairOperationClass>();
		//Creators	= reader.Read<Role>();
		Approval	= reader.Read<ApprovalRequirement>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Operation);
		//writer.Write(Creators);
		writer.Write(Approval);
	}

// 	public override bool ValidateProposal(FairExecution execution, out string error)
// 	{
//		if(Site.CreationPolicies.TryGetValue(Change, out var c) && c.SequenceEqual(Creators) &&
//			Site.ApprovalPolicies.TryGetValue(Change, out var a) && a == Approval)
//		{	
//			error = AlreadyExists;
//			return false;
//		}
//
//		if(Change == FairOperationClass.SitePolicyChange)
//		{
//			error = NotAvailable;
//			return false;
//		}
//		
//		error = null;
//		return true;
// 	}

	public override void Execute(FairExecution execution)
	{
		if(!Site.Restrictions.First(i => i.OperationClass == Operation).Flags.HasFlag(PolicyFlag.ChangableApproval))
		{
			Error = Denied;
			return;
		}

		//if(!Site.Restrictions.First(i => i.OperationClass == Operation).Creators.HasFlag(Creators))
		//{
		//	Error = Denied;
		//	return;
		//}

 		var s = Site;

		s.Policies = [..s.Policies];
		
		var i = Array.FindIndex(s.Policies, i => i.OperationClass == Operation);

		s.Policies[i] = new Policy(Operation, s.Policies[i].Creators, Approval);
	}
}