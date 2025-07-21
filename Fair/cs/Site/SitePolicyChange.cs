namespace Uccs.Fair;

public class SitePolicyChange : VotableOperation
{
	public FairOperationClass	Change { get; set; }
	public ChangePolicy			Policy { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Site={Site}, Change+{Change}, Policy={Policy}";
	
	public override void Read(BinaryReader reader)
	{
		Change	= reader.Read<FairOperationClass>();
		Policy	= reader.Read<ChangePolicy>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Change);
		writer.Write(Policy);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SitePolicyChange;
		
		return o.Change == Change;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(Site.ApprovalPolicies.TryGetValue(Change, out var p) && p == Policy)
		{	
			error = AlreadyExists;
			return false;
		}
		
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Site;
 
		s.ApprovalPolicies = new(s.ApprovalPolicies);
		s.ApprovalPolicies[Change] = Policy;
	}
}