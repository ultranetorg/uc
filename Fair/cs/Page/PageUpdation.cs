namespace Uccs.Fair;

public class PageUpdation : FairOperation
{
	public EntityId				Page { get; set; }
	public PageField			Flags { get; set; }
	public PageContent			Content { get; set; }
	public PagePermissions		Permissions { get; set; }
	public EntityId[]			Pages { get; set; }

	public override bool		IsValid(Mcv mcv) => Page != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public PageUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Page	= reader.Read<EntityId>();
		Flags	= (PageField)reader.ReadByte();
		
		if(Flags.HasFlag(PageField.Content))		Content	= reader.Read<PageContent>();
		if(Flags.HasFlag(PageField.Permissions))	Permissions	= reader.Read<PagePermissions>();
		if(Flags.HasFlag(PageField.Pages))			Pages	= reader.ReadArray<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Page);
		writer.Write((byte)Flags);

		if(Flags.HasFlag(PageField.Content))		writer.Write(Content);
		if(Flags.HasFlag(PageField.Permissions))	writer.Write(Permissions);
		if(Flags.HasFlag(PageField.Pages))			writer.Write(Pages);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequirePageAccess(round, Page, out var s, out var p) == false)
			return;

		var c = round.AffectPage(Page);

		if(Flags.HasFlag(PageField.Content))
		{
			if(Content.Type == PageType.Product)
			{
				if(RequireProduct(round, Content.Product.Product, out var a, out var prod) == false)
					return;
			}
			
			p.Fields |= PageField.Content;
			p.Content = Content;
		}
						
		if(Flags.HasFlag(PageField.Permissions))
		{
			p.Fields |= PageField.Permissions;
			p.Permissions	= Permissions;
		}
		
		if(Flags.HasFlag(PageField.Pages))
		{
			p.Fields |= PageField.Pages;
			p.Pages	= Pages;
		}
	}
}