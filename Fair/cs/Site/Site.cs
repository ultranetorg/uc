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

public class Site : IBinarySerializable, IEnergyHolder, ISpaceHolder, ISpaceConsumer
{
	public EntityId				Id { get; set; }
	public string				Title { get; set; }
	public EntityId[]			Moderators { get; set; }
	public EntityId[]			Categories { get; set; }

	public Time					Expiration { get; set; }
	public long					Space { get; set; }
	public long					Spacetime { get; set; }
	public long					Energy { get; set; }
	public byte					EnergyThisPeriod { get; set; }
	public long					EnergyNext { get; set; }
	public int					ModerationReward  { get; set; }

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();
		Title			= reader.ReadUtf8();
		Moderators		= reader.ReadArray<EntityId>();
		Categories		= reader.ReadArray<EntityId>();

		Expiration			= reader.Read<Time>();
		Space				= reader.Read7BitEncodedInt64();
		Spacetime		 	= reader.Read7BitEncodedInt64();
		Energy		 		= reader.Read7BitEncodedInt64();
		EnergyThisPeriod 	= reader.ReadByte();
		EnergyNext	 		= reader.Read7BitEncodedInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Moderators);
		writer.Write(Categories);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);
	}
}
