namespace Uccs.Fair;

public class CategoryMovement : FairOperation
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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireCategoryAccess(execution, Category, out var c))
			return;

		c = execution.Categories.Affect(Category);

		if(c.Parent != null)
		{
			var p = execution.Categories.Affect(c.Parent);

			p.Categories = p.Categories.Where(i => i != c.Id).ToArray();
		}

		if(Parent == null)
		{
			var s = execution.Sites.Affect(c.Site);
			s.Categories = [..s.Categories, c.Id];
		} 
		else
		{
			if(!RequireCategory(execution, Parent, out var p))
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

		PayEnergyBySite(execution, c.Site);
	}
}