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


		s.ApprovalPolicies[FairOperationClass.SiteNicknameChange]			= ChangePolicy.ElectedByAuthorsMajority;
		s.ApprovalPolicies[FairOperationClass.SiteAvatarChange]				= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SitePolicyChange]				= ChangePolicy.ElectedByAuthorsMajority;

		s.ApprovalPolicies[FairOperationClass.SiteDescriptionChange]		= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteModeratorsChange]			= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.SiteAuthorsChange]			= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryCreation]				= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryDeletion]				= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ApprovalPolicies[FairOperationClass.CategoryAvatarChange]			= ChangePolicy.ElectedByModeratorsUnanimously;

		s.ApprovalPolicies[FairOperationClass.PublicationCreation]			= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationDeletion]			= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationUpdation]			= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationPublish]			= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.PublicationRemoveFromChanged]	= ChangePolicy.AnyModerator;

		s.ApprovalPolicies[FairOperationClass.ReviewCreation]				= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewStatusChange]			= ChangePolicy.AnyModerator;
		s.ApprovalPolicies[FairOperationClass.ReviewEditModeration]			= ChangePolicy.AnyModerator;


		Signer.Sites = [..Signer.Sites, s.Id];

		execution.Prolong(Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
