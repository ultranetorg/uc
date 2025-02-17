namespace Uccs.Fair;

public class SiteCreation : FairOperation
{
	public string				Title { get; set; }
	public byte					Years {get; set;}

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(SiteChanges.Description) || (Data.Length <= Site.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public SiteCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Title = reader.ReadString();
		Years = reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Title);
		writer.Write(Years);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		var s = round.CreateSite(Signer);

		s.Title = Title;
		s.Moderators = [Signer.Id];

		Signer.Sites ??= [];
		Signer.Sites = [..Signer.Sites, s.Id];

		Prolong(round, Signer, s, Time.FromYears(Years));
	}
}
