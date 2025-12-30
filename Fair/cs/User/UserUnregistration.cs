namespace Uccs.Fair;

public class UserUnregistration : VotableOperation
{
	public new  AutoId			User { get; set; }

	public override string		Explanation => $"Site={Site}";
	
	public UserUnregistration()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		User = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(User);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is UserUnregistration o && o.User == User;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!Site.Users.Contains(User))
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

		s.Users = s.Users.Remove(User);
		base.User.Sites = base.User.Sites.Remove(s.Id);

		//if(base.User.AllocationSponsor == new EntityAddress((byte)FairTable.Site, s.Id))
		//{
		//	execution.FreeForever(s, execution.Net.EntityLength);
		//}
	}
}
