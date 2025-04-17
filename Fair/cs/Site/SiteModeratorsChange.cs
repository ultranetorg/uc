namespace Uccs.Fair;

public class SiteModeratorsChange : VotableOperation
{
	public EntityId				Site { get; set; }
	public EntityId[]			Additions { get; set; }
	public EntityId[]			Removals { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Id}";
	
	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<EntityId>();
		Additions	= reader.ReadArray<EntityId>();
		Removals	= reader.ReadArray<EntityId>();
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

 		var s = execution.AffectSite(Site);
 
 		s.Moderators = [..s.Moderators, ..Additions];
 
 		foreach(var i in Removals)
 			s.Moderators = s.Moderators.Remove(i);
	}
}