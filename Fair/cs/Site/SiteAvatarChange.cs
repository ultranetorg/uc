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
		writer.WriteBytes(Image);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var f = execution.AllocateFile(Site.Id, Site.Avatar, Site, Site, Image);

		Site.Avatar = f?.Id;
	}
}
