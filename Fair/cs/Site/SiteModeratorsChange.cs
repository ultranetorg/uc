namespace Uccs.Fair;

public class SiteModeratorsChange : VotableOperation
{
	public AutoId				Site { get; set; }
	public AutoId[]			Additions { get; set; }
	public AutoId[]			Removals { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Id}";
	
	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Additions	= reader.ReadArray<AutoId>();
		Removals	= reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Additions);
		writer.Write(Removals);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteModeratorsChange;
		
		foreach(var i in Additions)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
	
		foreach(var i in Removals)
			if(o.Additions.Contains(i) || o.Removals.Contains(i))
				return true;
				
		return false;
	}

 	public override bool ValidProposal(FairExecution execution)
 	{
 		if(!RequireSite(execution, Site, out var s))
			return false;

		foreach(var i in Additions)
			if(s.Moderators.Contains(i))
				return false;
	
		foreach(var i in Removals)
			if(!s.Moderators.Contains(i))
				return false;
	
		return true;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
	 		if(!RequireSiteModeratorAccess(execution, Site, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.SiteModeratorsChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}

 		var s = execution.Sites.Affect(Site);
 
 		s.Moderators = [..s.Moderators, ..Additions];
 
 		foreach(var i in Removals)
 			s.Moderators = s.Moderators.Remove(i);

		PayEnergyBySite(execution, s.Id);
	}
}