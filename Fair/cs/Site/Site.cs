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

public enum ModerationPermissions : byte
{
	None,
	ElectModerators	= 0b_0000_0001
}

public class Site : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer
{
	public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;

	public EntityId					Id { get; set; }
	public string					Title { get; set; }
	public int						ModerationReward { get; set; }
	public ModerationPermissions	ModerationPermissions { get; set; }

	public short					Expiration { get; set; }
	public long						Space { get; set; }
	public long						Spacetime { get; set; }

	public EntityId[]				Authors { get; set; }
	public EntityId[]				Moderators { get; set; }
	public EntityId[]				Categories { get; set; }
	public EntityId[]				Disputes { get; set; }

	public long						Energy { get; set; }
	public byte						EnergyThisPeriod { get; set; }
	public long						EnergyNext { get; set; }
	public long						Bandwidth { get; set; }
	public short					BandwidthExpiration { get; set; } = -1;
	public long						BandwidthToday { get; set; }
	public short					BandwidthTodayTime { get; set; }
	public long						BandwidthTodayAvailable { get; set; }

	public bool IsSpendingAuthorized(Round round, EntityId signer)
	{
		return Moderators[0] == signer; /// TODO : Owner only
	}

	public static bool IsExpired(Site a, Time time) 
	{
		return time.Days > a.Expiration;
	}

	public static bool CanRenew(Site author, Time time)
	{
		return !IsExpired(author, time) && time.Days > author.Expiration - RenewalPeriod; /// renewal by owner: renewal is allowed during last year olny
	}

	public void Read(BinaryReader reader)
	{
		Id						= reader.Read<EntityId>();
		Title					= reader.ReadUtf8();
		ModerationReward		= reader.Read7BitEncodedInt();
		ModerationPermissions	= reader.ReadEnum<ModerationPermissions>();

		Expiration				= reader.ReadInt16();
		Space					= reader.Read7BitEncodedInt64();
		Spacetime				= reader.Read7BitEncodedInt64();

		Authors					= reader.ReadArray<EntityId>();
		Moderators				= reader.ReadArray<EntityId>();
		Categories				= reader.ReadArray<EntityId>();
		Disputes				= reader.ReadArray<EntityId>();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Title);
		writer.Write7BitEncodedInt(ModerationReward);
		writer.WriteEnum(ModerationPermissions);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write(Authors);
		writer.Write(Moderators);
		writer.Write(Categories);
		writer.Write(Disputes);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}
}
