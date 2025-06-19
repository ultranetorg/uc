namespace Uccs.Fair;

public class CategoryCreation : VotableOperation
{
	public AutoId				Site { get; set; }
	public AutoId				Parent { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(McvNet net) => (Site != null || Parent != null) && (Site == null || Parent == null) && Title != null;
	public override string		Explanation => $"{GetType().Name} Title={Title} Parent={Parent}, Site={Site}";

	public override void Read(BinaryReader reader)
	{
		Site = reader.ReadNullable<AutoId>();
		Parent = reader.ReadNullable<AutoId>();
		Title = reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteNullable(Site);
		writer.WriteNullable(Parent);
		writer.WriteUtf8(Title);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidProposal(FairExecution execution)
 	{
		if(Parent == null)
 		{
			if(!RequireSite(execution, Site, out var s))
 				return false;
		}
		else
		{
 			if(!RequireCategory(execution, Parent, out var c))
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
	 		if(!RequireModeratorAccess(execution, Site, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.CategoryCreation] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
			
			PayEnergyBySite(execution, x.Id);
		}

		Site s;
	
		if(Parent == null)
		{
			s = execution.Sites.Affect(Site);
			var c = execution.Categories.Create(s);
			
			c.Site = s.Id;
			c.Title = Title;
			
			s.Categories = [..s.Categories, c.Id];

			Allocate(execution, s, s, execution.Net.EntityLength);
		}
		else
		{
			var p = execution.Categories.Affect(Parent);
			s = execution.Sites.Affect(p.Site);
			var c = execution.Categories.Create(s);
			
			c.Site = p.Site;
			c.Parent = Parent;
			c.Title = Title;

			p = execution.Categories.Affect(Parent);
			p.Categories = [..p.Categories, c.Id];
		
			Allocate(execution, s, s, execution.Net.EntityLength);
		}

	}
}