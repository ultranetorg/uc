namespace Uccs.Fair;

public class PageEntry : Page, ITableEntry
{
	public BaseId		BaseId => Id;
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public PageEntry()
	{
	}

	public PageEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public PageEntry Clone()
	{
		return new(Mcv){Id			= Id,
						Site		= Site,
						Flags		= Flags,
						Permissions = Permissions,
						Content		= Content,
						Pages		= [..Pages],
						Comments	= [..Comments]};
	}

	public void ReadMain(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Flags		= (PageFlags)reader.ReadByte();
		
		if(Flags.HasFlag(PageFlags.Content))		Content	= reader.Read<PageContent>();
		if(Flags.HasFlag(PageFlags.Permissions))	Permissions	= reader.Read<PagePermissions>();
		if(Flags.HasFlag(PageFlags.Pages))			Pages		= reader.ReadArray<EntityId>();
		if(Flags.HasFlag(PageFlags.Comments))		Comments	= reader.ReadArray<EntityId>();
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.Write((byte)Flags);

		if(Flags.HasFlag(PageFlags.Content))		writer.Write(Content);
		if(Flags.HasFlag(PageFlags.Permissions))	writer.Write(Permissions);
		if(Flags.HasFlag(PageFlags.Pages))			writer.Write(Pages);
		if(Flags.HasFlag(PageFlags.Comments))		writer.Write(Comments);
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

}

