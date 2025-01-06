namespace Uccs.Fair;

public enum PageType
{
	None,
	Static,
	Product
}

public enum PageField
{
	None,
	Content		= 0b0000_0001,
	Permissions = 0b0000_0010,
	Pages		= 0b0000_0100,
	Comments	= 0b0000_1000,
}

public class PagePermissions : IBinarySerializable
{
	public static PagePermissions Parse(string v)
	{
		return new PagePermissions();
	}

	public void Read(BinaryReader reader)
	{
	}

	public void Write(BinaryWriter writer)
	{
	}
}

public class PageContent : IBinarySerializable
{
	public PageContent()
	{
	}

	public PageContent(PageType type, object data)
	{
		Data = data;
		Type = type;
	}

	public object			Data { get; set; }
	public PageType			Type { get; set; }

	public string			Plain => Data as string;
	public ProductData		Product => Data as ProductData;

	public void Read(BinaryReader reader)
	{
		Type		= (PageType)reader.ReadByte();
		Data		= Type	switch
							{
								PageType.Static =>	reader.ReadUtf8(),
								PageType.Product => reader.Read<ProductData>(),
								_ => throw new IntegrityException()
							};
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)Type);
		
		switch(Type)
		{
			case PageType.Static	: writer.WriteUtf8(Plain); break;
			case PageType.Product	: writer.Write(Product); break;
			default					: throw new IntegrityException();
		};
	}
}

public class ProductData : IBinarySerializable
{
	public EntityId			Product { get; set; }
	public string[]			Sections { get; set; }

	public ProductData()
	{
	}

	public ProductData(EntityId product, string[] sections)
	{
		Product = product;
		Sections = sections;
	}

	public void Read(BinaryReader reader)
	{
		Product		= reader.Read<EntityId>();
		Sections	= reader.ReadArray(reader.ReadUtf8);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Sections, writer.WriteUtf8);
	}
}

public class Page : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Site { get; set; }
	public PageField		Fields { get; set; }
	public PagePermissions	Permissions { get; set; }
	public PageContent		Content { get; set; }
	public EntityId[]		Pages { get; set; }
	public EntityId[]		Comments { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Fields		= (PageField)reader.ReadByte();
		
		if(Fields.HasFlag(PageField.Permissions))	Permissions	= reader.Read<PagePermissions>();
		if(Fields.HasFlag(PageField.Content))		Content		= reader.Read<PageContent>();
		if(Fields.HasFlag(PageField.Pages))			Pages		= reader.ReadArray<EntityId>();
		if(Fields.HasFlag(PageField.Comments))		Comments	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.Write((byte)Fields);

		if(Fields.HasFlag(PageField.Permissions))	writer.Write(Permissions);
		if(Fields.HasFlag(PageField.Content))		writer.Write(Content);
		if(Fields.HasFlag(PageField.Pages))			writer.Write(Pages);
		if(Fields.HasFlag(PageField.Comments))		writer.Write(Comments);
	}
}
