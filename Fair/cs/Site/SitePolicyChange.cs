namespace Uccs.Fair;

public class SitePolicyChange : SiteOperation
{
	public FairOperationClass	Change { get; set; }
	public Role[]				Creators { get; set; }
	public ApprovalPolicy		Approval { get; set; }

	public override bool		IsValid(McvNet net) => Creators.Distinct().Count() == Creators.Length;
	public override string		Explanation => $"Site={Site}, Change+{Change}, Policy={Approval}";
	
	public override void Read(BinaryReader reader)
	{
		Change		= reader.Read<FairOperationClass>();
		Creators	= reader.ReadArray(() => reader.Read<Role>());
		Approval	= reader.Read<ApprovalPolicy>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Change);
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

		s.CreationPolicies = new(s.CreationPolicies);
		s.CreationPolicies[Change] = Creators;
 
		s.ApprovalPolicies = new(s.ApprovalPolicies);
		s.ApprovalPolicies[Change] = Approval;
	}
}