namespace Uccs.Fair;

public class SitePolicyChange : VotableOperation
{
	public AutoId				Site { get; set; }
	public FairOperationClass	Change { get; set; }
	public ChangePolicy			Policy { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Id}";
	
	public override void Read(BinaryReader reader)
	{
		Site	= reader.Read<AutoId>();
		Change	= reader.Read<FairOperationClass>();
		Policy	= reader.Read<ChangePolicy>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Change);
		writer.Write(Policy);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SitePolicyChange;
		
		return o.Change == Change;
	}

 	public override bool ValidProposal(FairExecution execution)
 	{
 		if(!RequireSite(execution, Site, out var s))
 			return false;

		return s.ChangePolicies.TryGetValue(Change, out var p) && p != Policy;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
	 		if(!RequireSiteModeratorAccess(execution, Site, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.SitePolicyChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}

 		var s = execution.AffectSite(Site);
 
		s.ChangePolicies = new(s.ChangePolicies);
		s.ChangePolicies[Change] = Policy;
	}
}