namespace Uccs.Fair;

public class SiteCreation : FairOperation
{
	public string				Title { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(SiteChanges.Description) || (Data.Length <= Site.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public SiteCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Title	= reader.ReadUtf8();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Title);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		var s = round.CreateSite(Signer);

		s.Title = Title;
		s.Moderators = [Signer.Id];

		Signer.Sites = Signer.Sites == null ? [s.Id] : [..Signer.Sites, s.Id];
	}
}
