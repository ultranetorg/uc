namespace Uccs.Fair;

public class CategoryCreation : VotableOperation
{
	public AutoId				Parent { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(McvNet net) => Title != null && Title.Length > 0 && Title.Length <= Fair.TitleLengthMaximum;
	public override string		Explanation => $"Title={Title} Parent={Parent}";

	public override void Read(Reader reader)
	{
		Parent = reader.ReadNullable<AutoId>();
		Title = reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteNullable(Parent);
		writer.WriteUtf8(Title);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(Parent != null)
 		{
 			if(!CategoryExists(execution, Parent, out var c, out error))
 				return false;
		}

		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		if(Parent == null)
		{
			var c = execution.Categories.Create(Store);
			
			c.Store = Store.Id;
			c.Title = Title;
			
			Store.Categories = [..Store.Categories, c.Id];
		}
		else
		{
			var p = execution.Categories.Affect(Parent);
			var c = execution.Categories.Create(Store);
			
			c.Store = p.Store;
			c.Parent = Parent;
			c.Title = Title;

			p = execution.Categories.Affect(Parent);
			p.Categories = [..p.Categories, c.Id];
		
		}

		execution.Allocate(Store, Store, execution.Net.EntityLength);
	}
}