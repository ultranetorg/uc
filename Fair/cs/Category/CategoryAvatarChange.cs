namespace Uccs.Fair;

public class CategoryAvatarChange : VotableOperation
{
	public AutoId				Category { get; set; } 
	public AutoId				File { get; set; }

	public override string		Explanation => $"Category={Category}, File={File}";
	
	public CategoryAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Category= reader.Read<AutoId>();
		File	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.Write(File);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as CategoryAvatarChange;

		return o.Category == Category;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!CategoryExists(execution, Category, out var c, out error))
			return false;

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
		var c = execution.Categories.Affect(Category);
		var f = execution.Files.Affect(File);
			
		c.Avatar = File;
		f.Refs++;
	}
}
