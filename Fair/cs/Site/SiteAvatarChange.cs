namespace Uccs.Fair;

public class SiteAvatarChange : VotableOperation
{
	public byte[]				Image { get; set; }

	public override string		Explanation => $"Site={Site} Image={Image?.Length}";
	
	public SiteAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Image	= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Image);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return true;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var s = execution.Sites.Affect(Site.Id);
			
		var f = execution.AllocateFile(s.Id, s.Avatar, s, s, Image);

		s.Avatar = f?.Id;
	}
}
