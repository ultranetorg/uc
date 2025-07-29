namespace Uccs.Fair;

public class SiteAvatarChange : VotableOperation
{
	public AutoId				File { get; set; }

	public override string		Explanation => $"Site={Site}, File={File}";
	
	public SiteAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		File = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(File);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as SiteAvatarChange;

		return o.Site == Site;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!FileExists(execution, File, out var f, out error))
			return false;

		if(f.Owner.Id != Site.Id || f.Owner.Table != FairTable.Site)
		{
			error = DoesNotBelogToSite;
			return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var f = execution.Files.Affect(File);
			
		Site.Avatar = f.Id;
		f.Refs++;
	}
}
