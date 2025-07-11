namespace Uccs.Fair;

public class CategoryDeletion : VotableOperation
{
	public AutoId				Category { get; set; }

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

	public override bool Overlaps(VotableOperation other)
	{
		return other is CategoryDeletion o && o.Category == Category;
	}

 	public override bool ValidateProposal(FairExecution execution)
 	{
		if(!CategoryExists(execution, Category, out var c, out _))
	 		return false;

		if(c.Publications.Any() || c.Categories.Any())
		{
			return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var c = execution.Categories.Affect(Category);

		if(c.Parent != null)
		{
			var p = execution.Categories.Affect(c.Parent);

			p.Categories = p.Categories.Remove(c.Id);
		}
	}
}