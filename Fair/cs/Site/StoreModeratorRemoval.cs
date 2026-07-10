namespace Uccs.Fair;

public class StoreModeratorRemoval : VotableOperation
{
	public AutoId				Moderator { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Store={Store}, Moderator={Moderator}";
	
	public override void Read(Reader reader)
	{
		Moderator = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Moderator);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as StoreModeratorRemoval;
		
		if(o.Moderator == Moderator)
			return true;
				
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!Store.Moderators.Any(m => m.User == Moderator))
		{
			error = NotFound;
			return false;
		}
	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Store;
 
		s.Moderators = s.Moderators.Remove(s.Moderators.First(m => m.User == Moderator));
	}
}