namespace Uccs.Fair;

public class ProductDeletion : FairOperation
{
	public AutoId				Product { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Id}";

	public ProductDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Product = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(RequireProductAccess(execution, Product, out var a, out var p) == false)
			return;

		a = execution.Authors.Affect(p.Author);
		a.Products = a.Products.Where(i => i != Product).ToArray();

		execution.Products.Affect(Product).Deleted = true;

		Free(execution, Signer, a, execution.Net.EntityLength + p.Length);
	}
}