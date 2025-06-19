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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(Signer.AllocationSponsor != null)
		{
			Error = NotAllowedForFreeAccount;
			return;
		}

		var s = execution.Sites.Create(Signer);

		s.Title			= Title;
		s.Space			= execution.Net.EntityLength;
		s.Moderators	= [Signer.Id];

		s.ChangePolicies[FairOperationClass.NicknameChange]			= ChangePolicy.ElectedByAuthorsMajority;
		s.ChangePolicies[FairOperationClass.SiteDescriptionChange]	= ChangePolicy.ElectedByModeratorsUnanimously;

		s.ChangePolicies[FairOperationClass.SitePolicyChange]		= ChangePolicy.ElectedByAuthorsMajority;
		s.ChangePolicies[FairOperationClass.SiteAuthorsChange]		= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.SiteModeratorsChange]	= ChangePolicy.ElectedByModeratorsUnanimously;

		s.ChangePolicies[FairOperationClass.CategoryCreation]		= ChangePolicy.ElectedByModeratorsUnanimously;

		s.ChangePolicies[FairOperationClass.PublicationApproval]		= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationUpdation]		= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationProductChange]	= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.PublicationCategoryChange]	= ChangePolicy.AnyModerator;

		s.ChangePolicies[FairOperationClass.ReviewStatusChange]			= ChangePolicy.AnyModerator;
		s.ChangePolicies[FairOperationClass.ReviewTextModeration]		= ChangePolicy.AnyModerator;

		Signer.Sites = [..Signer.Sites, s.Id];

		Prolong(execution, Signer, s, Time.FromYears(Years));

		execution.SiteTitles.Index(s.Id, Title);
	}
}
