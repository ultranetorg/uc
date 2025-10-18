namespace Uccs.Fair;

public enum ApprovalRequirement : byte
{
	None, AnyModerator, ModeratorsMajority, AllModerators, PublishersMajority
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
	public long			EnergyLimit { get; set; }
	public long			SpacetimeLimit { get; set; }

	public const long	Unlimit = -1;

	public void Read(BinaryReader reader)
	{
		Author			= reader.Read<AutoId>();
		BannedTill		= reader.Read<Time>();
		EnergyLimit		= reader.Read7BitEncodedInt64();
		SpacetimeLimit	= reader.Read7BitEncodedInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.Write(BannedTill);
		writer.Write7BitEncodedInt64(EnergyLimit);
		writer.Write7BitEncodedInt64(SpacetimeLimit);
	}
}

public class Policy : IBinarySerializable
{
	public FairOperationClass	Operation;
	public Role[]				Creators;
	public ApprovalRequirement	Approval;

	public Policy()
	{
	}

	public Policy(FairOperationClass operation, Role[] creators, ApprovalRequirement approval)
	{
		Operation = operation;
		Creators = creators;
		Approval = approval;
	}

	public void Read(BinaryReader reader)
	{
		Operation	= reader.Read<FairOperationClass>();
		Creators	= reader.ReadArray(() => reader.Read<Role>());
		Approval	= reader.Read<ApprovalRequirement>();
		
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Operation);
		writer.Write(Creators, i => writer.Write(i));
		writer.Write(Approval);
	}
}

public class Site : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry, IExpirable
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
	public PerpetualSurvey[]		PerpetualSurveys { get; set; }
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
	
	public Policy[]					Policies { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	FairMcv							Mcv;

	public PerpetualSurvey			FindPerpetualSurvey(FairOperationClass operation) => PerpetualSurveys.FirstOrDefault(i => i.Options[0].Operation is SitePolicyChange o && o.Operation == operation);
	public sbyte					FindPerpetualSurveyIndex(FairOperationClass operation) => (sbyte)Array.FindIndex(PerpetualSurveys, i => i.Options[0].Operation is SitePolicyChange o && o.Operation == operation);

	public bool IsSpendingAuthorized(Execution executions, AutoId signer)
	{
		return  false; /// Moderators[0] == signer; /// TODO : Owner only
	}

	public bool IsExpired(Site a, Time time) 
	{
		return time.Days > a.Expiration;
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
					
					Policies				= Policies,

					Expiration				= Expiration,
					Space					= Space,
					Spacetime				= Spacetime,

					PublicationsCount		= PublicationsCount,

					Publishers				= Publishers,
					Moderators				= Moderators,
					Categories				= Categories,
					Proposals				= Proposals,
					PerpetualSurveys		= PerpetualSurveys,
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
		
		Policies					= reader.ReadArray<Policy>();

		Expiration					= reader.ReadInt16();
		Space						= reader.Read7BitEncodedInt64();
		Spacetime					= reader.Read7BitEncodedInt64();
		
		PublicationsCount			= reader.Read7BitEncodedInt();

		Publishers					= reader.ReadArray<Publisher>();
		Moderators					= reader.ReadArray<Moderator>();
		Users						= reader.ReadArray<AutoId>();
		Categories					= reader.ReadArray<AutoId>();
		Proposals					= reader.ReadArray<AutoId>();
		PerpetualSurveys			= reader.ReadArray<PerpetualSurvey>();
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
		
		writer.Write(Policies);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write7BitEncodedInt(PublicationsCount);

		writer.Write(Publishers);
		writer.Write(Moderators);
		writer.Write(Users);
		writer.Write(Categories);
		writer.Write(Proposals);
		writer.Write(PerpetualSurveys);
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
		return Policies.First(i => i.Operation == operation).Approval == ApprovalRequirement.PublishersMajority;
	}

	public bool IsDiscussion(FairOperationClass operation)
	{
		var a = Policies.First(i => i.Operation == operation).Approval;

		return a == ApprovalRequirement.AnyModerator || a == ApprovalRequirement.ModeratorsMajority || a == ApprovalRequirement.AllModerators;
	}

}
