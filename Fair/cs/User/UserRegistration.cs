using System.Numerics;

namespace Uccs.Fair;

public class UserRegistration : VotableOperation
{
	public override string		Explanation => $"Store={Store}";
	
	public UserRegistration()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is UserRegistration o && o.User.Id == User.Id;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(execution.Transaction.Nonce == 0)
		{
			error = NotAllowedForNewUser;
			return false;
		}

		if(Store.Users.Contains(User.Id))
		{
			error = AlreadyExists;
			return false;
		}

		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var s = Store;

		s.Users = [..s.Users, User.Id];

		User.Stores = [..User.Stores, Store.Id];
//
//		if(Pow != null)
//		{	
//			User.AllocationSponsor = new EntityAddress((byte)FairTable.Store, s.Id);
//			execution.AllocateForever(s, execution.Net.EntityLength);
//		}
//		else
//			execution.AllocateForever(User, execution.Net.EntityLength);
	}
}
