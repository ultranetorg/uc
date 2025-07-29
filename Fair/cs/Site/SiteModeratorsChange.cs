namespace Uccs.Fair;

public class SiteModeratorsChange : VotableOperation
{
	public AutoId[]				Additions { get; set; }
	public AutoId[]				Removals { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Site={Site}, Additions={Additions.Length}, Removals={Removals.Length}";
	
	public override void Read(BinaryReader reader)
	{
		Additions	= reader.ReadArray<AutoId>();
		Removals	= reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
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

 	 public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Additions)
		{
			if(Site.Moderators.Any(m => m.Account == i))
			{	
				error = AlreadyExists;
				return false;
			}

			if(!AccountExists(execution, i, out _, out error))
				return false;
		}
	
		foreach(var i in Removals)
			if(!Site.Moderators.Any(m => m.Account == i))
			{
				error = NotFound;
				return false;
			}
	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Site;
 
 		s.Moderators = [..s.Moderators, ..Additions.Select(i => new Moderator {Account = i})];
 
 		foreach(var i in Removals)
 			s.Moderators = s.Moderators.Remove(s.Moderators.First(m => m.Account == i));
	}
}