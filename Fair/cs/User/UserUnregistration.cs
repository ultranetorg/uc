namespace Uccs.Fair;

public class UserUnregistration : VotableOperation
{
	public new AutoId			User { get; set; }

	public override string		Explanation => $"User={User}";
	
	public UserUnregistration()
	{
	}

	public override bool IsValid(McvNet net)
	{
		return true;
	}

	public override void Read(Reader reader)
	{
		User = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(User);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is UserUnregistration o && o.User == User;
	}

	public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!Store.Users.Contains(User))
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

		s.Users = s.Users.Remove(User);
		
		var u = execution.AffectUser(User);

		u.Stores = u.Stores.Remove(s.Id);
	}
}