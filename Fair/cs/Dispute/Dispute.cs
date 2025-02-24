namespace Uccs.Fair;

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

public class Proposal : IBinarySerializable, IEquatable<Proposal>
{
	public SiteChange	Change { get; set; }
	public object		Value { get; set; }

	public static bool	operator == (Proposal left, Proposal right) => left.Equals(right);
	public static bool	operator != (Proposal left, Proposal right) => !(left == right);

	public override bool Equals(object x)
	{
		return Equals(x as Proposal);
	}

	public bool Equals(Proposal other)
	{
		return other is not null && Change == other.Change && Value.Equals(other.Value);
	}
	
	public void Read(BinaryReader reader)
	{
		Change	= reader.ReadEnum<SiteChange>();

		Value = Change switch
					   {
							SiteChange.AddModerator		=> reader.Read<EntityId>(),
							SiteChange.RemoveModerator	=> reader.Read<EntityId>(),
							_							=> throw new IntegrityException()
					   };
		
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteEnum(Change);

		switch(Change)
		{
			case SiteChange.AddModerator	: writer.Write(Value as EntityId); break;
			case SiteChange.RemoveModerator	: writer.Write(Value as EntityId); break;
			default							: throw new IntegrityException();
		}
	}
}

public enum DisputeFlags
{
	Resolved	= 0b0001,
	Referendum	= 0b0010,
}

public class Dispute : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Site { get; set; }
	public DisputeFlags		Flags { get; set; }
	public Proposal			Proposal { get; set; }
	public EntityId[]		Pros { get; set; }
	public EntityId[]		Cons { get; set; }
	public Time				Expirtaion { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Flags		= reader.ReadEnum<DisputeFlags>();
		Proposal	= reader.Read<Proposal>();
		Pros		= reader.ReadArray<EntityId>();
		Cons		= reader.ReadArray<EntityId>();
		Expirtaion	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteEnum(Flags);
		writer.Write(Proposal);
		writer.Write(Pros);
		writer.Write(Cons);
		writer.Write(Expirtaion);
	}
}
