namespace Uccs.Fair;

public class SiteModeratorAddition : VotableOperation
{
	public AutoId[]				Candidates { get; set; }

	public override bool		IsValid(McvNet net) => Candidates.Length > 0 && Candidates.Length <= 10;
	public override string		Explanation => $"Site={Site}, Additions={Candidates.Length}";
	
	public override void Read(BinaryReader reader)
	{
		Candidates	= reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Candidates);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteModeratorAddition;
		
		foreach(var i in Candidates)
			if(o.Candidates.Contains(i))
				return true;
				
		return false;
	}

 	 public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Candidates)
		{
			if(Site.Moderators.Any(m => m.User == i))
			{	
				error = AlreadyExists;
				return false;
			}

			if(!AccountExists(execution, i, out _, out error))
				return false;
		}
	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Site;
 
 		s.Moderators = [..s.Moderators, ..Candidates.Select(i => new Moderator {User = i})];
	}
}