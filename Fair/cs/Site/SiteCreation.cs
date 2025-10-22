using System.Text;

namespace Uccs.Fair;

public class SiteCreation : FairOperation
{
	public string				Title { get; set; }
	public byte					Years {get; set;}

	public override bool		IsValid(McvNet net) => true; // !Changes.HasFlag(SiteChanges.Description) || (Data.Length <= Site.DescriptionLengthMax);
	public override string		Explanation => $"{Title}, Years{Years}";

	public SiteCreation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Title = reader.ReadString();
		Years = reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Title);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		var s = execution.Sites.Create(Signer);

		s.Title			= Title;
		s.PoWComplexity	= 172;
		s.Space			= execution.Net.EntityLength;
		s.Moderators	= [new Moderator {Account = Signer.Id}];

		s.Policies =   [new (FairOperationClass.SiteModeratorAddition,			Role.Moderator|Role.Publisher,					ApprovalRequirement.PublishersMajority),	
						new (FairOperationClass.SiteModeratorRemoval,			Role.Moderator|Role.Publisher,					ApprovalRequirement.PublishersMajority),
						new (FairOperationClass.SiteNicknameChange,				Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.SiteTextChange,					Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.SiteAvatarChange,				Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.SiteAuthorsChange,				Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
																																								
						new (FairOperationClass.CategoryCreation,				Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryDeletion,				Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryTypeChange,				Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.CategoryAvatarChange,			Role.Moderator|Role.Publisher, 					ApprovalRequirement.AnyModerator),
																																								
						new (FairOperationClass.PublicationCreation,			Role.Moderator|Role.Publisher|Role.Candidate, 	ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationDeletion,			Role.Moderator|Role.Publisher,					ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationUpdation,			Role.Moderator, 								ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationPublish,				Role.Moderator, 								ApprovalRequirement.AnyModerator),
						new (FairOperationClass.PublicationRemoveFromChanged,	Role.Moderator, 								ApprovalRequirement.AnyModerator),
																																								
						new (FairOperationClass.UserRegistration,				Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.UserDeletion,					Role.Moderator, 								ApprovalRequirement.AnyModerator),
																																								
						new (FairOperationClass.ReviewCreation,					Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.ReviewEdit,						Role.User, 										ApprovalRequirement.AnyModerator),
						new (FairOperationClass.ReviewStatusChange,				Role.Moderator, 								ApprovalRequirement.AnyModerator)];

		s.PerpetualSurveys = s.Policies.Select(i =>	{
 														var z = new PerpetualSurvey();
 
														z.LastWin = -1;
 														z.Options = Enum.GetValues<ApprovalRequirement>()	.Where(i => i != ApprovalRequirement.None)
																											.Select(a => new SurveyOption(	new SitePolicyChange
																																			{
																																				Operation = i.Operation, 
																																				Creators = Site.Restrictions.First(j => j.Operation == i.Operation).Creators,
																																				Approval = a
																																			}))
																											.ToArray();
														return z;
													}).ToArray();;


		Signer.ModeratedSites = [..Signer.ModeratedSites, s.Id];

		execution.Prolong(Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
