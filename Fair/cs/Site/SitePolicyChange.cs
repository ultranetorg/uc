namespace Uccs.Fair;

public class SitePolicyChange : SiteOperation
{
	public FairOperationClass	Operation { get; set; }
	public Role[]				Creators { get; set; }
	public ApprovalRequirement	Approval { get; set; }

	public override bool		IsValid(McvNet net) =>	Enum.IsDefined<FairOperationClass>(Operation) && 
														Creators.All(i => Enum.IsDefined<Role>(i)) && Creators.Distinct().Count() == Creators.Length &&
														Enum.IsDefined<ApprovalRequirement>(Approval);

	public override string		Explanation => $"Site={Site}, Operation+{Operation}, Approval={Approval}";
	
	public override void Read(BinaryReader reader)
	{
		Operation	= reader.Read<FairOperationClass>();
		Creators	= reader.ReadArray(() => reader.Read<Role>());
		Approval	= reader.Read<ApprovalRequirement>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Operation);
		writer.Write(Creators, i => writer.Write(i));
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
 		var s = Site;

		s.Policies = [..s.Policies];
		
		var i = Array.FindIndex(s.Policies, i => i.Operation == Operation);

		s.Policies[i] = new Policy(Operation, Creators, Approval);
	}
}