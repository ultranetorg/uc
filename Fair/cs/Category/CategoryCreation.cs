namespace Uccs.Fair;

public class CategoryCreation : FairOperation
{
	public EntityId				Site { get; set; }
	public EntityId				Parent { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(Mcv mcv) => (Site != null || Parent != null) && (Site == null || Parent == null) && Title != null;
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

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(Parent == null)
		{
			if(!RequireSiteAccess(round, Site, out var s))
				return;
				
			var c = round.CreateCategory(s);
			
			c.Site = s.Id;
			c.Title = Title;
			
			s = round.AffectSite(s.Id);
			s.Categories = [..s.Categories, c.Id];

			Allocate(round, Signer, s, mcv.Net.EntityLength);
		} 
		else
		{
			if(!RequireCategory(round, Parent, out var p))
				return;

			var s = round.AffectSite(p.Site);
			var c = round.CreateCategory(s);
			
			c.Site = p.Site;
			c.Parent = Parent;
			c.Title = Title;

			p = round.AffectCategory(Parent);
			p.Categories = [..p.Categories, c.Id];
		
			Allocate(round, Signer, s, mcv.Net.EntityLength);
		}
	}
}