namespace Uccs.Smp;

public enum SiteType : uint
{
	None,
	Store
}

public enum Actor
{
	None,
	Owner,
	Creator,
	SiteUser
}

// public class Security : IBinarySerializable
// {
// 	public OrderedDictionary<TopicChange, Actor[]>	Permissions;
// 
// 	public static Security Parse(string v)
// 	{
// 		return new Security {Permissions = new (new Xon(v).Nodes.ToDictionary(i => Enum.Parse<TopicChange>(i.Name), 
// 																			  i => i.Get<string>().Split(',').Select(i => Enum.Parse<Actor>(i)).ToArray()))};
// 	}
// 
// 	public void Read(BinaryReader reader)
// 	{
// 		Permissions = reader.ReadOrderedDictionary(() => (TopicChange)reader.Read7BitEncodedInt64(),
// 												   () => reader.ReadArray(() => (Actor)reader.Read7BitEncodedInt64()));
// 	}
// 
// 	public void Write(BinaryWriter writer)
// 	{
// 		writer.Write(Permissions, i =>	{ 
// 											writer.Write7BitEncodedInt64((long)i.Key); 
// 											writer.Write(i.Value, i => writer.Write7BitEncodedInt64((long)i)); 
// 										});
// 	}
// 	
// 	public Security Clone()
// 	{
// 		return new Security {Permissions = new (Permissions)};
// 	}
// }

public class Site : IBinarySerializable
{
	public EntityId		Id { get; set; }
	public SiteType		Type { get; set; }
	public string		Title { get; set; }
	public EntityId[]	Owners { get; set; }
	//public EntityId		Root { get; set; }
	public EntityId[]	Categories { get; set; }

	enum Field
	{
		Root = 0b1
	}

	public void Read(BinaryReader reader)
	{
		//var f = (Field)reader.ReadByte();

		Id		= reader.Read<EntityId>();
		Type	= reader.ReadEnum<SiteType>();
		Title	= reader.ReadUtf8();
		Owners	= reader.ReadArray<EntityId>();
		//if(f.HasFlag(Field.Root)) Root = reader.Read<EntityId>();
		Categories	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		//writer.Write((byte)(Root != null ? Field.Root : 0));

		writer.Write(Id);
		writer.WriteEnum(Type);
		writer.Write(Title);
		writer.Write(Owners);
		//if(Root != null) writer.Write(Root);
		writer.Write(Categories);
	}
}
