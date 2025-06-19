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

 	public override bool ValidProposal(FairExecution execution)
 	{
		if(!RequireCategory(execution, Category, out var c))
	 		return false;

		if(c.Publications.Any() || c.Categories.Any())
		{
			return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
	 		if(!RequireCategoryAccess(execution, Category, out var x, out var s))
 				return;

	 		if(s.ChangePolicies[FairOperationClass.CategoryDeletion] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}

			PayEnergyBySite(execution, s.Id);
		}

		var c = execution.Categories.Affect(Category);

		if(c.Parent != null)
		{
			var p = execution.Categories.Affect(c.Parent);

			p.Categories = p.Categories.Remove(c.Id);
		}

	}
}