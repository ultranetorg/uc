namespace Uccs.Fair;

public class CategoryMovement : VotableOperation
{
	public AutoId				Category { get; set; }
	public AutoId				Parent { get; set; }

	public override bool		IsValid(McvNet net) => Category != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{GetType().Name}, {Parent}";

	public override void Read(BinaryReader reader)
	{
		Category	= reader.Read<AutoId>();
		Parent		= reader.ReadNullable<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.WriteNullable(Parent);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is CategoryMovement o && o.Category == Category;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!CategoryExists(execution, Category, out var _, out error))
	 		return false;

		if(Parent != null && !CategoryExists(execution, Parent, out var _, out error))
	 	{
			error = NotFound;
			return false;
		}
//
//	 	if(Parent != null && !RequireCategoryAccess(execution, Parent, out var _, out var _))
//	 		return false;

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var c = execution.Categories.Affect(Category);

		if(c.Parent != null)
		{
			var p = execution.Categories.Affect(c.Parent);

			p.Categories = p.Categories.Where(i => i != c.Id).ToArray();
		}

		if(Parent == null)
		{
			Site.Categories = [..Site.Categories, c.Id];
		} 
		else
		{
			if(!CategoryExists(execution, Parent, out var p, out Error))
				return;

			if(p.Site != c.Site)
			{
				Error = NotFound;
				return;
			}

			c.Parent = p.Id;

			p = execution.Categories.Affect(p.Id);
			p.Categories = [..p.Categories, c.Id];
		}
	}
}