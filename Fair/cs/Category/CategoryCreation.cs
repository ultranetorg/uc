namespace Uccs.Fair;

public class CategoryCreation : VotableOperation
{
	public AutoId				Parent { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(McvNet net) => Title.Length <= Fair.TitleLengthMaximum;
	public override string		Explanation => $"Title={Title} Parent={Parent}";

	public override void Read(BinaryReader reader)
	{
		Parent = reader.ReadNullable<AutoId>();
		Title = reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteNullable(Parent);
		writer.WriteUtf8(Title);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution)
 	{
		if(Parent != null)
 		{
 			if(!CategoryExists(execution, Parent, out var c, out _))
 				return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var s = execution.Sites.Affect(Site.Id);

		if(Parent == null)
		{
			var c = execution.Categories.Create(s);
			
			c.Site = s.Id;
			c.Title = Title;
			
			s.Categories = [..s.Categories, c.Id];

			execution.Allocate(s, s, execution.Net.EntityLength);
		}
		else
		{
			var p = execution.Categories.Affect(Parent);
			var c = execution.Categories.Create(s);
			
			c.Site = p.Site;
			c.Parent = Parent;
			c.Title = Title;

			p = execution.Categories.Affect(Parent);
			p.Categories = [..p.Categories, c.Id];
		
			execution.Allocate(s, s, execution.Net.EntityLength);
		}
	}
}