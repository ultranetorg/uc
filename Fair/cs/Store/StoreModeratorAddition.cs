namespace Uccs.Fair;

public class StoreModeratorAddition : VotableOperation
{
	public AutoId[]				Candidates { get; set; }

	public override bool		IsValid(McvNet net) => Candidates.Length > 0 && Candidates.Length <= 10;
	public override string		Explanation => $"Store={Store}, Additions={Candidates.Length}";
	
	public override void Read(Reader reader)
	{
		Candidates	= reader.ReadArray<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Candidates);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as StoreModeratorAddition;
		
		foreach(var i in Candidates)
			if(o.Candidates.Contains(i))
				return true;
				
		return false;
	}

 	 public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		foreach(var i in Candidates)
		{
			if(Store.Moderators.Any(m => m.User == i))
			{	
				error = AlreadyExists;
				return false;
			}

			if(!UserExists(execution, i, out _, out error))
				return false;
		}
	
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
 		var s = Store;
 
 		s.Moderators = [..s.Moderators, ..Candidates.Select(i => new Moderator {User = i})];
	}
}