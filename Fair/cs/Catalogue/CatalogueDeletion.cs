namespace Uccs.Fair;

public class CatalogueDeletion : FairOperation
{
	public EntityId				Catalogue { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public CatalogueDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Catalogue = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Catalogue);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireCatalogueAccess(round, Catalogue, out var c) == false)
			return;

		round.AffectCatalogue(Catalogue).Deleted = true;
		
		foreach(var i in c.Topics)
		{
			var car = round.AffectTopic(i);
			car.Deleted = true;
		}

		foreach(var i in c.Owners)
		{
			var a = round.AffectAccount(i);
			a.Catalogues = a.Catalogues.Where(i => i == Catalogue).ToArray();
		}
		//Free(d, r.Length);
	}
}