namespace Uccs.Fair;

public class CatalogueCreation : FairOperation
{
	public string				Title { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(CatalogueChanges.Description) || (Data.Length <= Catalogue.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public CatalogueCreation()
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
		var c = round.AffectCatalogue(Signer);

		c.Title = Title;
		c.Owners = [Signer.Id];

		var a = Signer as FairAccountEntry;

		a.Catalogues = a.Catalogues == null ? [c.Id] : [..a.Catalogues, c.Id];
	}
}
