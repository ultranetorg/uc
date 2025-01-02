namespace Uccs.Fair;

public class PageEntry : Page, ITableEntry
{
	public BaseId		BaseId => Id;
	public PageField	Affecteds;
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
						Fields		= Fields,
						Permissions = Permissions,
						Content		= Content,
						Pages		= Pages,
						Comments	= Comments};
	}

	public void ReadMain(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Fields		= (PageField)reader.ReadByte();
		
		if(Fields.HasFlag(PageField.Content))		Content		= reader.Read<PageContent>();
		if(Fields.HasFlag(PageField.Permissions))	Permissions	= reader.Read<PagePermissions>();
		if(Fields.HasFlag(PageField.Pages))			Pages		= reader.ReadArray<EntityId>();
		if(Fields.HasFlag(PageField.Comments))		Comments	= reader.ReadArray<EntityId>();
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.Write((byte)Fields);

		if(Fields.HasFlag(PageField.Content))		writer.Write(Content);
		if(Fields.HasFlag(PageField.Permissions))	writer.Write(Permissions);
		if(Fields.HasFlag(PageField.Pages))			writer.Write(Pages);
		if(Fields.HasFlag(PageField.Comments))		writer.Write(Comments);
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

