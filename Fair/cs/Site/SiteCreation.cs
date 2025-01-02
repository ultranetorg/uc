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
		Title = reader.ReadUtf8();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Title);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		var c = round.AffectSite(Signer);

		c.Title = Title;
		c.Owners = [Signer.Id];

		var a = Signer as FairAccountEntry;

		a.Sites = a.Sites == null ? [c.Id] : [..a.Sites, c.Id];
	}
}
