namespace Uccs.Fair;

public enum ApprovalPolicy : byte
{
	None, AnyModerator, ModeratorsMajority, AllModerators, AuthorsMajority
}

public enum Role : byte
{
	None, Candidate, Moderator, Publisher, User
}

public class Moderator : IBinarySerializable
{
	public AutoId		Account { get; set; }
	public Time			BannedTill { get; set; }

	public void Read(BinaryReader reader)
	{
		Account		= reader.Read<AutoId>();
		BannedTill	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Account);
		writer.Write(BannedTill);
	}
}

public class Publisher : IBinarySerializable
{
	public AutoId		Author { get; set; }
	public Time			BannedTill { get; set; }

	public void Read(BinaryReader reader)
	{
		Author		= reader.Read<AutoId>();
		BannedTill	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.Write(BannedTill);
	}
}

public class Site : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry
{
	public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;
	public const int				PoWLength = 32;

	public AutoId					Id { get; set; }
	public string					Nickname { get; set; }
	public string					Title { get; set; }
	public string					Slogan { get; set; }
	public string					Description { get; set; }
	public int						ModerationReward { get; set; }
	public int						PoWComplexity { get; set; }
	public AutoId					Avatar  { get; set; }

	public short					Expiration { get; set; }
	public long						Space { get; set; }
	public long						Spacetime { get; set; }

	public Publisher[]				Publishers { get; set; }
	public Moderator[]				Moderators { get; set; }
	public AutoId[]					Categories { get; set; }
	public AutoId[]					Proposals { get; set; }
	public AutoId[]					UnpublishedPublications { get; set; }
	public AutoId[]					ChangedPublications { get; set; }
	public AutoId[]					Files { get; set; }
	public AutoId[]					Users { get; set; }

	public int						PublicationsCount { get; set; }
	public int						CandidateRequestFee { get; set; }

	public long						Energy { get; set; }
	public byte						EnergyThisPeriod { get; set; }
	public long						EnergyNext { get; set; }
	public long						Bandwidth { get; set; }
	public short					BandwidthExpiration { get; set; } = -1;
	public long						BandwidthToday { get; set; }
	public short					BandwidthTodayTime { get; set; }
	public long						BandwidthTodayAvailable { get; set; }
	
	public OrderedDictionary<FairOperationClass, Role[]>			CreationPolicies { get; set; }
	public OrderedDictionary<FairOperationClass, ApprovalPolicy>	ApprovalPolicies { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	FairMcv							Mcv;

	public bool IsSpendingAuthorized(Execution executions, AutoId signer)
	{
		return  false; /// Moderators[0] == signer; /// TODO : Owner only
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
		var a = new Site(Mcv)  
				{
					Id						= Id,
					Title					= Title,
					Slogan					= Slogan,
					Description				= Description,
					Nickname				= Nickname,
					ModerationReward		= ModerationReward,
					CandidateRequestFee		= CandidateRequestFee,
					Avatar					= Avatar,
					PoWComplexity			= PoWComplexity,
					
					CreationPolicies		= CreationPolicies,
					ApprovalPolicies		= ApprovalPolicies,

					Expiration				= Expiration,
					Space					= Space,
					Spacetime				= Spacetime,

					PublicationsCount		= PublicationsCount,

					Publishers					= Publishers,
					Moderators				= Moderators,
					Categories				= Categories,
					Proposals				= Proposals,
					UnpublishedPublications	= UnpublishedPublications,
					ChangedPublications		= ChangedPublications,
					Files					= Files,
					Users					= Users
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
		Title						= reader.ReadUtf8();
		Slogan						= reader.ReadUtf8Nullable();
		Description					= reader.ReadUtf8Nullable();
		ModerationReward			= reader.Read7BitEncodedInt();
		CandidateRequestFee			= reader.Read7BitEncodedInt();
		PoWComplexity				= reader.Read7BitEncodedInt();
		Avatar						= reader.ReadNullable<AutoId>();
		
		CreationPolicies			= reader.ReadOrderedDictionary(() => reader.Read<FairOperationClass>(), () => reader.ReadArray(() => reader.Read<Role>()));
		ApprovalPolicies			= reader.ReadOrderedDictionary(() => reader.Read<FairOperationClass>(), () => reader.Read<ApprovalPolicy>());

		Expiration					= reader.ReadInt16();
		Space						= reader.Read7BitEncodedInt64();
		Spacetime					= reader.Read7BitEncodedInt64();
		
		PublicationsCount			= reader.Read7BitEncodedInt();

		Publishers						= reader.ReadArray<Publisher>();
		Moderators					= reader.ReadArray<Moderator>();
		Users						= reader.ReadArray<AutoId>();
		Categories					= reader.ReadArray<AutoId>();
		Proposals					= reader.ReadArray<AutoId>();
		UnpublishedPublications		= reader.ReadArray<AutoId>();
		ChangedPublications			= reader.ReadArray<AutoId>();
		Files						= reader.ReadArray<AutoId>();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Nickname);
		writer.WriteUtf8(Title);
		writer.WriteUtf8Nullable(Slogan);
		writer.WriteUtf8Nullable(Description);
		writer.Write7BitEncodedInt(ModerationReward);
		writer.Write7BitEncodedInt(CandidateRequestFee);
		writer.Write7BitEncodedInt(PoWComplexity);
		writer.WriteNullable(Avatar);
		
		writer.Write(CreationPolicies, i => { writer.Write(i.Key); writer.Write(i.Value, i => writer.Write(i)); });
		writer.Write(ApprovalPolicies, i => { writer.Write(i.Key); writer.Write(i.Value); });

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write7BitEncodedInt(PublicationsCount);

		writer.Write(Publishers);
		writer.Write(Moderators);
		writer.Write(Users);
		writer.Write(Categories);
		writer.Write(Proposals);
		writer.Write(UnpublishedPublications);
		writer.Write(ChangedPublications);
		writer.Write(Files);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}


	//public bool IsReferendum(Proposal proposal)
	//{
	//	return Sites.Find(proposal.Site).ApprovalPolicies[Enum.Parse<FairOperationClass>(proposal.Option.GetType().Name)] == ApprovalPolicy.ElectedByAuthorsMajority;
	//}

	public bool IsReferendum(FairOperationClass operation)
	{
		return ApprovalPolicies[operation] == ApprovalPolicy.AuthorsMajority;
	}

	public bool IsDiscussion(FairOperationClass operation)
	{
		return ApprovalPolicies[operation] == ApprovalPolicy.AnyModerator || ApprovalPolicies[operation] == ApprovalPolicy.ModeratorsMajority || ApprovalPolicies[operation] == ApprovalPolicy.AllModerators;
	}

}
