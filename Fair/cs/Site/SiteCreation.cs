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


		s.CreationPolicies[FairOperationClass.SiteNicknameChange]			= [Role.Publisher];
		s.CreationPolicies[FairOperationClass.SiteTextChange]				= [Role.Publisher];
		s.CreationPolicies[FairOperationClass.SiteAvatarChange]				= [Role.Publisher];
		s.CreationPolicies[FairOperationClass.SitePolicyChange]				= [Role.Publisher];
		s.CreationPolicies[FairOperationClass.SiteAuthorsChange]			= [Role.Publisher];
		s.CreationPolicies[FairOperationClass.SiteModeratorsChange]			= [Role.Publisher];
		
		s.CreationPolicies[FairOperationClass.UserRegistration]				= [Role.User];
		s.CreationPolicies[FairOperationClass.UserDeletion]					= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.CategoryCreation]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryDeletion]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryTypeChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryAvatarChange]			= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.PublicationCreation]			= [Role.Publisher, Role.Moderator, Role.Candidate];
		s.CreationPolicies[FairOperationClass.PublicationDeletion]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationUpdation]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationPublish]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationRemoveFromChanged]	= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.ReviewCreation]				= [Role.User];
		s.CreationPolicies[FairOperationClass.ReviewEdit]					= [Role.User];
		s.CreationPolicies[FairOperationClass.ReviewStatusChange]			= [Role.Moderator];


		s.ApprovalPolicies[FairOperationClass.SiteNicknameChange]			= ApprovalPolicy.AuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteTextChange]				= ApprovalPolicy.AuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteAvatarChange]				= ApprovalPolicy.AuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SitePolicyChange]				= ApprovalPolicy.AuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteAuthorsChange]			= ApprovalPolicy.AuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteModeratorsChange]			= ApprovalPolicy.AuthorsMajority;
		
		s.ApprovalPolicies[FairOperationClass.UserRegistration]				= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.UserDeletion]					= ApprovalPolicy.AnyModerator;
		
		s.ApprovalPolicies[FairOperationClass.CategoryCreation]				= ApprovalPolicy.AllModerators;
		s.ApprovalPolicies[FairOperationClass.CategoryDeletion]				= ApprovalPolicy.AllModerators;
		s.ApprovalPolicies[FairOperationClass.CategoryTypeChange]			= ApprovalPolicy.AllModerators;
		s.ApprovalPolicies[FairOperationClass.CategoryAvatarChange]			= ApprovalPolicy.AllModerators;

		s.ApprovalPolicies[FairOperationClass.PublicationCreation]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationDeletion]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationUpdation]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationPublish]			= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationRemoveFromChanged]	= ApprovalPolicy.AnyModerator;

		s.ApprovalPolicies[FairOperationClass.ReviewCreation]				= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewEdit]					= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewStatusChange]			= ApprovalPolicy.AnyModerator;


		Signer.ModeratedSites = [..Signer.ModeratedSites, s.Id];

		execution.Prolong(Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
