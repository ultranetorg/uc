namespace Uccs.Fair;

public class SiteModeratorRemoval : VotableOperation
{
	public AutoId				Moderator { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Site={Site}, Moderator={Moderator}";
	
	public override void Read(BinaryReader reader)
	{
		Moderator = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Moderator);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteModeratorRemoval;
		
		if(o.Moderator == Moderator)
			return true;
				
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!Site.Moderators.Any(m => m.Account == Moderator))
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
 
		s.Moderators = s.Moderators.Remove(s.Moderators.First(m => m.Account == Moderator));
	}
}