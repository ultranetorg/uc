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

		s.CreationPolicies[FairOperationClass.SitePolicyChange]				= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteAuthorsChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.SiteModeratorsChange]			= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.CategoryCreation]				= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.PublicationCreation]			= [Role.Moderator, Role.Author];
		s.CreationPolicies[FairOperationClass.PublicationDeletion]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationUpdation]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationPublish]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.PublicationRemoveFromChanged]	= [Role.Moderator];

		s.CreationPolicies[FairOperationClass.ReviewStatusChange]			= [Role.Moderator];
		s.CreationPolicies[FairOperationClass.ReviewEditModeration]			= [Role.Moderator];



		s.ChangePolicies[FairOperationClass.SiteNicknameChange]				= ChangePolicy.ElectedByAuthorsMajority;
		s.ChangePolicies[FairOperationClass.SitePolicyChange]				= ChangePolicy.ElectedByAuthorsMajority;

		s.ChangePolicies[FairOperationClass.SiteDescriptionChange]			= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ChangePolicies[FairOperationClass.SiteModeratorsChange]			= ChangePolicy.ElectedByModeratorsUnanimously;
		s.ChangePolicies[FairOperationClass.CategoryCreation]				= ChangePolicy.ElectedByModeratorsUnanimously;

		s.ChangePolicies[FairOperationClass.SiteAuthorsChange]				= ChangePolicy.AnyModerator;

		s.ChangePolicies[FairOperationClass.PublicationCreation]			= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationDeletion]			= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationUpdation]			= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationPublish]				= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationRemoveFromChanged]	= ChangePolicy.AnyModerator;

		s.ChangePolicies[FairOperationClass.ReviewStatusChange]				= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.ReviewEditModeration]			= ChangePolicy.AnyModerator;


		Signer.Sites = [..Signer.Sites, s.Id];

		execution.Prolong(Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
