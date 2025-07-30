using System.Text;

namespace Uccs.Fair;

public class SiteCreation : FairOperation
{
	public string				Title { get; set; }
	public byte					Years {get; set;}

	public override bool		IsValid(McvNet net) => true; // !Changes.HasFlag(SiteChanges.Description) || (Data.Length <= Site.DescriptionLengthMax);
	public override string		Explanation => $"{GetType().Name}";

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
		s.PoWComplexity	= 170;
		s.Space			= execution.Net.EntityLength;
		s.Moderators	= [new Moderator {Account = Signer.Id}];


		s.CreationPolicies[FairOperationClass.SiteNicknameChange]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteTextChange]				= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteAvatarChange]				= [Role.Citizen, Role.Moderator];

		s.CreationPolicies[FairOperationClass.SitePolicyChange]				= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteAuthorsChange]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteModeratorsChange]			= [Role.Citizen, Role.Moderator];
		
		s.CreationPolicies[FairOperationClass.UserRegistration]				= [Role.User];
		s.CreationPolicies[FairOperationClass.UserDeletion]					= [Role.Citizen, Role.Moderator];

		s.CreationPolicies[FairOperationClass.CategoryCreation]				= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryDeletion]				= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryTypeChange]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryAvatarChange]			= [Role.Citizen, Role.Moderator];

		s.CreationPolicies[FairOperationClass.PublicationCreation]			= [Role.Citizen, Role.Moderator, Role.Author];
		s.CreationPolicies[FairOperationClass.PublicationDeletion]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationUpdation]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationPublish]			= [Role.Citizen, Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationRemoveFromChanged]	= [Role.Citizen, Role.Moderator];

		s.CreationPolicies[FairOperationClass.ReviewCreation]				= [Role.User];
		s.CreationPolicies[FairOperationClass.ReviewEdit]					= [Role.User];
		s.CreationPolicies[FairOperationClass.ReviewStatusChange]			= [Role.Citizen, Role.Moderator];


		s.ApprovalPolicies[FairOperationClass.SiteNicknameChange]			= ApprovalPolicy.ElectedByAuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteAvatarChange]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SitePolicyChange]				= ApprovalPolicy.ElectedByAuthorsMajority;
		
		s.ApprovalPolicies[FairOperationClass.UserRegistration]				= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.UserDeletion]					= ApprovalPolicy.AnyModerator;

		s.ApprovalPolicies[FairOperationClass.SiteTextChange]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteModeratorsChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteAuthorsChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;
		
		s.ApprovalPolicies[FairOperationClass.CategoryCreation]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryDeletion]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryTypeChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryAvatarChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;

		s.ApprovalPolicies[FairOperationClass.PublicationCreation]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationDeletion]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationUpdation]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationPublish]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationRemoveFromChanged]	= ApprovalPolicy.AnyModerator;

		s.ApprovalPolicies[FairOperationClass.ReviewCreation]				= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewStatusChange]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewEditModeration]			= ApprovalPolicy.AnyModerator;


		Signer.ModeratedSites = [..Signer.ModeratedSites, s.Id];

		execution.Prolong(Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
