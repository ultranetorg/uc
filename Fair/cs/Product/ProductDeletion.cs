namespace Uccs.Fair;

public class ProductDeletion : FairOperation
{
	public EntityId				Product { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public ProductDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Product = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Product);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireSignerProduct(round, Product, out var d, out var r) == false)
			return;

		round.AffectProduct(Product).Deleted = true;

		Free(d, r.Length);
	}
}