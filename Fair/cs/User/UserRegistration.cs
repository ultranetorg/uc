using System.Numerics;

namespace Uccs.Fair;

public class UserRegistration : VotableOperation
{
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
		return other is UserRegistration o && o.User.Id == User.Id;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(execution.Transaction.Nonce == 0)
		{
			error = NotAllowedForNewUser;
			return false;
		}

		if(Site.Users.Contains(User.Id))
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

		s.Users = [..s.Users, User.Id];

		User.Sites = [..User.Sites, Site.Id];
//
//		if(Pow != null)
//		{	
//			User.AllocationSponsor = new EntityAddress((byte)FairTable.Site, s.Id);
//			execution.AllocateForever(s, execution.Net.EntityLength);
//		}
//		else
//			execution.AllocateForever(User, execution.Net.EntityLength);
	}
}
