namespace Uccs.Fair;

public class CategoryTypeChange : VotableOperation
{
	public AutoId				Category { get; set; } 
	public ProductType			Type { get; set; }

	public override string		Explanation => $"Category={Category} Type={Type}";
	
	public CategoryTypeChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Category	= reader.Read<AutoId>();
		Type		= reader.Read<ProductType>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.Write(Type);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as CategoryTypeChange;

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

		c.Type = Type;
	}
}
