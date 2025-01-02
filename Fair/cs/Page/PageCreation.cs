namespace Uccs.Fair;

public class PageCreation : FairOperation
{
	public EntityId				Site { get; set; }
	public PageFlags			Flags { get; set; }
	public PageContent			Content { get; set; }
	public PagePermissions		Permissions { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public PageCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Site	= reader.Read<EntityId>();
		Flags	= (PageFlags)reader.ReadByte();
		
		if(Flags.HasFlag(PageFlags.Content))		Content	= reader.Read<PageContent>();
		if(Flags.HasFlag(PageFlags.Permissions))	Permissions	= reader.Read<PagePermissions>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write((byte)Flags);

		if(Flags.HasFlag(PageFlags.Content))		writer.Write(Content);
		if(Flags.HasFlag(PageFlags.Permissions))	writer.Write(Permissions);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireSiteAccess(round, Site, out var cat) == false)
			return;

		if(Content.Type == PageType.Product)
		{
			if(RequireProduct(round, Content.Product.Product, out var a, out var p) == false)
				return;
		}

		var c = round.AffectPage(cat);

		c.Site			= Site;
		c.Flags			= Flags;
		c.Content		= Content;
		c.Permissions	= Permissions;

		cat = round.AffectSite(cat.Id);

		cat.Roots = cat.Roots == null ? [c.Id] : [..cat.Roots, c.Id];
	}
}