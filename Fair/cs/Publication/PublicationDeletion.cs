namespace Uccs.Fair;

public class PublicationDeletion : FairOperation
{
	public EntityId				Publication { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null;
	public override string		Description => $"{Id}";

	public PublicationDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublication(round, Publication, out var p))
			return;

		p = round.AffectPublication(Publication);
		p.Deleted = true;

 		var c = round.AffectCategory(p.Category);
 		c.Publications = c.Publications.Where(i => i != Publication).ToArray();

		///Free(d, r.Length);
	}
}