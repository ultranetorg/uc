namespace Uccs.Fair;

public class CategoryCreation : FairOperation
{
	public EntityId				Site { get; set; }
	public EntityId				Parent { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(McvNet net) => (Site != null || Parent != null) && (Site == null || Parent == null) && Title != null;
	public override string		Description => $"{GetType().Name} Title={Title} Parent={Parent}, Site={Site}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site = reader.ReadNullable<EntityId>();
		Parent = reader.ReadNullable<EntityId>();
		Title = reader.ReadUtf8();
	}

	public override void WriteConfirmed(BinaryWriter writer)
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
				
			var c = execution.CreateCategory(s);
			
			c.Site = s.Id;
			c.Title = Title;
			
			s = execution.AffectSite(s.Id);
			s.Categories = [..s.Categories, c.Id];

			Allocate(execution, Signer, s, execution.Net.EntityLength);
		} 
		else
		{
			if(!RequireCategory(execution, Parent, out var p))
				return;

			var s = execution.AffectSite(p.Site);
			var c = execution.CreateCategory(s);
			
			c.Site = p.Site;
			c.Parent = Parent;
			c.Title = Title;

			p = execution.AffectCategory(Parent);
			p.Categories = [..p.Categories, c.Id];
		
			Allocate(execution, Signer, s, execution.Net.EntityLength);
		}
	}
}