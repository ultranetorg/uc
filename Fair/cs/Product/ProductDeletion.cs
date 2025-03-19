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

	public override void Execute(FairExecution execution)
	{
		if(RequireProductAccess(execution, Product, out var a, out var p) == false)
			return;

		a = execution.AffectAuthor(p.Author);
		a.Products = a.Products.Where(i => i != Product).ToArray();

		execution.AffectProduct(Product).Deleted = true;

		Free(execution, Signer, a, execution.Net.EntityLength + p.Length);
	}
}