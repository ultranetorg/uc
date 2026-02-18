namespace Uccs.Fair;

public enum ApprovalRequirement : byte
{
	None, AnyModerator, ModeratorsMajority, AllModerators, PublishersMajority
}

public enum Role : byte
{
	None, 
	Moderator	= 0b0001, /// User.Id
	Candidate	= 0b0010, /// Author.Id
	Publisher	= 0b0100, /// Author.Id
	User		= 0b1000, /// User.Id
}

public class Moderator : IBinarySerializable
{
	public AutoId		User { get; set; }
	public Time			BannedTill { get; set; }

	public void Read(BinaryReader reader)
	{
		User		= reader.Read<AutoId>();
		BannedTill	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(User);
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


public enum PolicyFlag : byte
{
	None, 
	ChangableCreators	= 0b0000_0001,
	ChangableApproval	= 0b0000_0010,
	Infinite			= 0b0000_0100,
}

public class Policy : IBinarySerializable
{
	public FairOperationClass	OperationClass { get ; set; }
	public Role					Creators { get ; set; }
	public ApprovalRequirement	Approval { get ; set; }

	public Policy()
	{
	}

	public Policy(FairOperationClass operation, Role creators, ApprovalRequirement approval)
	{
		OperationClass = operation;
		Creators = creators;
		Approval = approval;
	}

	public void Read(BinaryReader reader)
	{
		OperationClass	= reader.Read<FairOperationClass>();
		Creators	= reader.Read<Role>();
		Approval	= reader.Read<ApprovalRequirement>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(OperationClass);
		writer.Write(Creators);
		writer.Write(Approval);
	}
}

public class Restiction
{
	public FairOperationClass	OperationClass { get ; set; }
	public Role					Creators { get ; set; }
	public PolicyFlag			Flags { get ; set; }

	public Restiction()
	{
	}

	public Restiction(FairOperationClass operation, Role creators, PolicyFlag flags)
	{
		OperationClass = operation;
		Creators = creators;
		Flags = flags;
	}
}

public class Site : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry, IExpirable
{
	public const int					PoWLength = 32;

	public AutoId						Id { get; set; }
	public string						Name { get; set; }
	public string						Title { get; set; }
	public string						Slogan { get; set; }
	public string						Description { get; set; }
	public int							ModerationReward { get; set; }
	public int							PoWComplexity { get; set; }
	public AutoId						Avatar  { get; set; }

	public short						Expiration { get; set; }
	public long							Space { get; set; }
	public long							Spacetime { get; set; }
	public bool							Free { get; set; }

	public Publisher[]					Publishers { get; set; }
	public Moderator[]					Moderators { get; set; }
	public AutoId[]						Categories { get; set; }
	public PerpetualSurvey[]			PerpetualSurveys { get; set; }
	public AutoId[]						Proposals { get; set; }
	public AutoId[]						UnpublishedPublications { get; set; }
	public AutoId[]						ChangedPublications { get; set; }
	public AutoId[]						Files { get; set; }
	public AutoId[]						Users { get; set; }

	public int							PublicationsCount { get; set; }
	public int							CandidateRequestFee { get; set; }

	public long							Energy { get; set; }
	public byte							EnergyThisPeriod { get; set; }
	public long							EnergyNext { get; set; }
	public int							Bandwidth { get; set; }
	public int							BandwidthExpiration { get; set; }
	public int							EnergyPeriod { get; set; }
	public int							EnergyRating { get; set; }
	
	public Policy[]						Policies { get; set; }

	public EntityId						Key => Id;
	public bool							Deleted { get; set; }
	FairMcv								Mcv;

	public static readonly Restiction[]	Restrictions;

	public PerpetualSurvey				FindPerpetualSurvey(FairOperationClass operation) => PerpetualSurveys.FirstOrDefault(i => i.Options[0].Operation is SiteApprovalPolicyChange o && o.Operation == operation);
	public sbyte						FindPerpetualSurveyIndex(FairOperationClass operation) => (sbyte)Array.FindIndex(PerpetualSurveys, i => i.Options[0].Operation is SiteApprovalPolicyChange o && o.Operation == operation);

	public bool IsSpendingAuthorized(Execution executions, AutoId signer)
	{
		return  false; /// Moderators[0] == signer; /// TODO : Owner only
	}

	public bool IsExpired(Site a, Time time) 
	{
		return time.Days > a.Expiration;
	}

	static Site()
	{
		Restrictions = [new (FairOperationClass.SiteModeratorAddition,			Role.Moderator|Role.Publisher,					PolicyFlag.ChangableCreators							 ),	
						new (FairOperationClass.SiteModeratorRemoval,			Role.Moderator|Role.Publisher,					PolicyFlag.ChangableCreators							 |PolicyFlag.Infinite),
						new (FairOperationClass.SiteNicknameChange,				Role.Moderator|Role.Publisher,					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.SiteTextChange,					Role.Moderator|Role.Publisher, 					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.SiteAvatarChange,				Role.Moderator|Role.Publisher, 					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.SiteAuthorsChange,				Role.Moderator|Role.Publisher,					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						//																																								 
						new (FairOperationClass.CategoryCreation,				Role.Moderator|Role.Publisher, 					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.CategoryDeletion,				Role.Moderator|Role.Publisher, 					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.CategoryTypeChange,				Role.Moderator|Role.Publisher,					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.CategoryAvatarChange,			Role.Moderator|Role.Publisher, 					PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						//																																								 
						new (FairOperationClass.PublicationCreation,			Role.Moderator|Role.Publisher|Role.Candidate, 	PolicyFlag.ChangableCreators|PolicyFlag.ChangableApproval),
						new (FairOperationClass.PublicationDeletion,			Role.Moderator|Role.Publisher,												 PolicyFlag.ChangableApproval),
						new (FairOperationClass.PublicationUpdation,			Role.Moderator, 															 PolicyFlag.ChangableApproval),
						new (FairOperationClass.PublicationPublish,				Role.Moderator, 															 PolicyFlag.ChangableApproval),
						new (FairOperationClass.PublicationUnpublish,			Role.Moderator, 															 PolicyFlag.ChangableApproval),
						//																																								 
						new (FairOperationClass.UserRegistration,				Role.User, 																	 PolicyFlag.ChangableApproval),
						new (FairOperationClass.UserUnregistration,				Role.Moderator, 															 PolicyFlag.ChangableApproval),
						//																																	  							
						new (FairOperationClass.ReviewCreation,					Role.User, 																	 PolicyFlag.ChangableApproval),
						new (FairOperationClass.ReviewEdit,						Role.User, 																	 PolicyFlag.ChangableApproval),
						new (FairOperationClass.ReviewStatusChange,				Role.Moderator, 															 PolicyFlag.ChangableApproval)];
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
					Name				= Name,
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
		Name					= reader.ReadUtf8();
		Title						= reader.ReadUtf8();
		Slogan						= reader.ReadUtf8();
		Description					= reader.ReadUtf8();
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
		writer.WriteUtf8(Name);
		writer.WriteUtf8(Title);
		writer.WriteUtf8(Slogan);
		writer.WriteUtf8(Description);
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
		return Policies.First(i => i.OperationClass == operation).Approval == ApprovalRequirement.PublishersMajority;
	}

	public bool IsDiscussion(FairOperationClass operation)
	{
		var a = Policies.First(i => i.OperationClass == operation).Approval;

		return a == ApprovalRequirement.AnyModerator || a == ApprovalRequirement.ModeratorsMajority || a == ApprovalRequirement.AllModerators;
	}

}
