using System.Text;

namespace Uccs.Fair;

public class StoreCreation : FairOperation
{
	public string				Title { get; set; }
	public byte					Years {get; set;}

	public override string		Explanation => $"{Title}, Years{Years}";

	public StoreCreation()
	{
	}

	public override bool IsValid(McvNet net)
	{
		if((Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void Read(Reader reader)
	{
		Title = reader.ReadUtf8();
		Years = reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(execution.Transaction.Nonce == 0)
		{
			Error = NotAllowedForNewUser;
			return;
		}

		var s = execution.Stores.Create(User);

		s.Title			= Title;
		s.PoWComplexity	= 172;
		s.Space			= execution.Net.EntityLength;
		s.Moderators	= [new Moderator {User = User.Id}];

		s.Policies =   [new (FairOperationClass.StoreModeratorAddition,	Role.Moderator|Role.Publisher,					ApprovalRequirement.PublishersMajority),	
						new (FairOperationClass.StoreModeratorRemoval,	Role.Moderator|Role.Publisher,					ApprovalRequirement.PublishersMajority),
						new (FairOperationClass.StoreNameChange,		Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.StoreInfoUpdation,		Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.StoreAvatarChange,		Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.StoreAuthorsRemoval,	Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
																																						
						new (FairOperationClass.CategoryCreation,		Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryDeletion,		Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryMovement,		Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryTypeChange,		Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryAvatarChange,	Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
																																						
						new (FairOperationClass.PublicationCreation,	Role.Moderator|Role.Publisher|Role.Candidate, 	ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationDeletion,	Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationUpdation,	Role.Moderator, 								ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationPublish,		Role.Moderator, 								ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationUnpublish,	Role.Moderator, 								ApprovalRequirement.AnyModerator),
																																						
						new (FairOperationClass.UserRegistration,		Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.UserUnregistration,		Role.Moderator, 								ApprovalRequirement.AnyModerator),
																																						
						new (FairOperationClass.ReviewCreation,			Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.ReviewEdit,				Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.ReviewStatusChange,		Role.Moderator, 								ApprovalRequirement.AnyModerator)];

		s.PerpetualSurveys = s.Policies.Select(i =>	{
 														var z = new PerpetualSurvey();
 
														z.LastWin = -1;
 														z.Options = Enum.GetValues<ApprovalRequirement>()	.Where(i => i != ApprovalRequirement.None)
																											.Select(a => new SurveyOption(	new StoreApprovalPolicyChange
																																			{
																																				Operation = i.OperationClass, 
																																				//Creators = Store.Restrictions.First(j => j.OperationClass == i.OperationClass).Creators,
																																				Approval = a
																																			}))
																											.ToArray();
														return z;
													}).ToArray();


		User.ModeratedStores = [..User.ModeratedStores, s.Id];

		execution.StoreTitles.Index(s.Id, Title);

		execution.Prolong(User, s, Time.FromYears(Years));
		execution.PayOperationEnergy(User);
	}
}
