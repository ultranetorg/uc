namespace Uccs.Fair;

public class UserRegistration : VotableOperation
{
	//public AutoId				Site { get; set; }

	public override string		Explanation => $"Site={Site}";
	
	public UserRegistration()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
	}

	public override void Write(BinaryWriter writer)
	{
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is UserRegistration o && o.Signer.Address == Signer.Address;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(Site.Users.Contains(Signer.Id))
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

		s.Users = [..s.Users, Signer.Id];

		Signer.Registrations = [..Signer.Registrations, Site.Id];

		if(execution.Transaction.Sponsored)
		{
			execution.AllocateForever(s, execution.Net.EntityLength);
		}
	}
}
