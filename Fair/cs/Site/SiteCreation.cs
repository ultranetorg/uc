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
		if(Signer.AllocationSponsor != null)
		{
			Error = NotAllowedForSponsoredAccount;
			return;
		}

		var s = execution.Sites.Create(Signer);

		s.Title			= Title;
		s.Space			= execution.Net.EntityLength;
		s.Moderators	= [Signer.Id];


		s.CreationPolicies[FairOperationClass.SiteNicknameChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteDescriptionChange]		= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteAvatarChange]				= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.SitePolicyChange]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteAuthorsChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteModeratorsChange]			= [Role.Moderator];
		
		s.CreationPolicies[FairOperationClass.UserRegistration]				= [Role.User];
		s.CreationPolicies[FairOperationClass.UserDeletion]					= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.CategoryCreation]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryDeletion]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.CategoryAvatarChange]			= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.PublicationCreation]			= [Role.Moderator, Role.Author];
		s.CreationPolicies[FairOperationClass.PublicationDeletion]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationUpdation]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationPublish]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationRemoveFromChanged]	= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.ReviewCreation]				= [Role.User];
		s.CreationPolicies[FairOperationClass.ReviewStatusChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.ReviewEditModeration]			= [Role.Moderator];


		s.ApprovalPolicies[FairOperationClass.SiteNicknameChange]			= ApprovalPolicy.ElectedByAuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteAvatarChange]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SitePolicyChange]				= ApprovalPolicy.ElectedByAuthorsMajority;
		
		s.ApprovalPolicies[FairOperationClass.UserRegistration]				= ApprovalPolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.UserDeletion]					= ApprovalPolicy.AnyModerator;

		s.ApprovalPolicies[FairOperationClass.SiteDescriptionChange]		= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteModeratorsChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteAuthorsChange]			= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryCreation]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryDeletion]				= ApprovalPolicy.ElectedByModeratorsUnanimously;
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
