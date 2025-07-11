namespace Uccs.Fair;

public class CategoryAvatarChange : FairOperation
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
		writer.Write(Image);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanModerateCategory(execution, Category, out var c, out var s, out Error))
			return;

		s = execution.Sites.Affect(s.Id);
		c = execution.Categories.Affect(Category);
			
		var f = execution.AllocateFile(c.Id, c.Avatar, s, s, Image);

		c.Avatar = f?.Id;
	}
}
