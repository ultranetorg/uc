namespace Uccs.Fair;

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
	public string		Title { get; set; }
	public EntityId[]	Moderators { get; set; }
	public EntityId[]	Categories { get; set; }

	enum Field
	{
		Root = 0b1
	}

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Title		= reader.ReadUtf8();
		Moderators	= reader.ReadArray<EntityId>();
		Categories	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Moderators);
		writer.Write(Categories);
	}
}
