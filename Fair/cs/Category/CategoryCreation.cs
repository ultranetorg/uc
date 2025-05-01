namespace Uccs.Fair;

public class CategoryCreation : FairOperation
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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(Parent == null)
		{
			if(!RequireSiteModeratorAccess(execution, Site, out var s))
				return;
				
			var c = execution.Categories.Create(s);
			
			c.Site = s.Id;
			c.Title = Title;
			
			s = execution.Sites.Affect(s.Id);
			s.Categories = [..s.Categories, c.Id];

			Allocate(execution, Signer, s, execution.Net.EntityLength);
		} 
		else
		{
			if(!RequireCategory(execution, Parent, out var p))
				return;

			var s = execution.Sites.Affect(p.Site);
			var c = execution.Categories.Create(s);
			
			c.Site = p.Site;
			c.Parent = Parent;
			c.Title = Title;

			p = execution.Categories.Affect(Parent);
			p.Categories = [..p.Categories, c.Id];
		
			Allocate(execution, Signer, s, execution.Net.EntityLength);
		}
	}
}