namespace Uccs.Fair;

public class CategoryDeletion : FairOperation
{
	public AutoId		Category { get; set; }

	public override bool		IsValid(McvNet net) => Category != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{Category}";

	public override void Read(BinaryReader reader)
	{
		Category	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireCategoryAccess(execution, Category, out var c))
			return;

		if(c.Publications.Any() || c.Categories.Any())
		{
			Error = NotEmpty;
			return;
		}

		c = execution.Categories.Affect(Category);

		if(c.Parent != null)
		{
			var p = execution.Categories.Affect(c.Parent);

			p.Categories = p.Categories.Remove(c.Id);
		}

		PayEnergyBySite(execution, c.Site);
	}
}