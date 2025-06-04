namespace Uccs.Fair;

public enum ChangePolicy : byte
{
	None, AnyModerator, ElectedByModeratorsMajority, ElectedByModeratorsUnanimously, ElectedByAuthorsMajority
}

public class Site : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry
{
	public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;

	public AutoId					Id { get; set; }
	public string					Nickname { get; set; }
	public string					Title { get; set; }
	public string					Description { get; set; }
	public int						ModerationReward { get; set; }
	
	public OrderedDictionary<FairOperationClass, ChangePolicy> ChangePolicies { get; set; }

	public short					Expiration { get; set; }
	public long						Space { get; set; }
	public long						Spacetime { get; set; }

	public AutoId[]					Authors { get; set; }
	public AutoId[]					Moderators { get; set; }
	public AutoId[]					Categories { get; set; }
	public AutoId[]					Disputes { get; set; }
	public AutoId[]					PendingPublications { get; set; }

	public int						PublicationsCount { get; set; }
	public int						AuthorPublicationRequestFee { get; set; }

	public long						Energy { get; set; }
	public byte						EnergyThisPeriod { get; set; }
	public long						EnergyNext { get; set; }
	public long						Bandwidth { get; set; }
	public short					BandwidthExpiration { get; set; } = -1;
	public long						BandwidthToday { get; set; }
	public short					BandwidthTodayTime { get; set; }
	public long						BandwidthTodayAvailable { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	FairMcv							Mcv;

	public bool IsSpendingAuthorized(Execution round, AutoId signer)
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

	public Site()
	{
	}

	public Site(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		var a = new Site(Mcv){	Id							= Id,
								Title						= Title,
								Description					= Description,
								Nickname					= Nickname,
								ModerationReward			= ModerationReward,
								AuthorPublicationRequestFee	= AuthorPublicationRequestFee,
								
								ChangePolicies				= ChangePolicies,

								Expiration					= Expiration,
								Space						= Space,
								Spacetime					= Spacetime,

								PublicationsCount			= PublicationsCount,

								Authors						= Authors,
								Moderators					= Moderators,
								Categories					= Categories,
								Disputes					= Disputes,
								PendingPublications			= PendingPublications
								};
		
		((IEnergyHolder)this).Clone(a);

		return a;
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}


	public void Read(BinaryReader reader)
	{
		Id							= reader.Read<AutoId>();
		Nickname					= reader.ReadUtf8();
		Description					= reader.ReadUtf8();
		Title						= reader.ReadUtf8();
		ModerationReward			= reader.Read7BitEncodedInt();
		AuthorPublicationRequestFee	= reader.Read7BitEncodedInt();
		
		ChangePolicies				= reader.ReadOrderedDictionary(() => reader.Read<FairOperationClass>(), () => reader.Read<ChangePolicy>());

		Expiration					= reader.ReadInt16();
		Space						= reader.Read7BitEncodedInt64();
		Spacetime					= reader.Read7BitEncodedInt64();
		
		PublicationsCount			= reader.Read7BitEncodedInt();

		Authors						= reader.ReadArray<AutoId>();
		Moderators					= reader.ReadArray<AutoId>();
		Categories					= reader.ReadArray<AutoId>();
		Disputes					= reader.ReadArray<AutoId>();
		PendingPublications			= reader.ReadArray<AutoId>();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Nickname);
		writer.WriteUtf8(Description);
		writer.WriteUtf8(Title);
		writer.Write7BitEncodedInt(ModerationReward);
		writer.Write7BitEncodedInt(AuthorPublicationRequestFee);
		
		writer.Write(ChangePolicies, i => { writer.Write(i.Key); writer.Write(i.Value); });

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write7BitEncodedInt(PublicationsCount);

		writer.Write(Authors);
		writer.Write(Moderators);
		writer.Write(Categories);
		writer.Write(Disputes);
		writer.Write(PendingPublications);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}
}
