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
		if(RequireProductAccess(round, Product, out var a, out var p) == false)
			return;

		a = round.AffectAuthor(p.AuthorId);
		a.Products = a.Products.Where(i => i != Product).ToArray();

		round.AffectProduct(Product).Deleted = true;

		Free(round, a, p.Length);
	}
}