namespace Uccs.Fair;

public class CategoryAvatarChange : VotableOperation
{
	public AutoId				Category { get; set; } 
	public byte[]				Image { get; set; }

	public override string		Explanation => $"Category={Category} Image={Image?.Length}";
	
	public CategoryAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Category	= reader.Read<AutoId>();
		Image		= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.WriteBytes(Image);
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

		if(c.Site != Site.Id)
		{
			error = DoesNotBelogToSite;
			return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var c = execution.Categories.Affect(Category);
			
		var f = execution.AllocateFile(c.Id, c.Avatar, Site, Site, Image);

		c.Avatar = f?.Id;
	}
}
