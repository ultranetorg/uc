namespace Uccs.Fair;

public class ProductDeletion : FairOperation
{
	public AutoId				Product { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Product={Product}";

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

	public override void Execute(FairExecution execution)
	{
		if(CanAccessProduct(execution, Product, out var a, out var p, out Error) == false)
			return;

		a = execution.Authors.Affect(p.Author);
		a.Products = a.Products.Where(i => i != Product).ToArray();

		execution.Products.Affect(Product).Deleted = true;

		execution.Free(a, a, execution.Net.EntityLength + p.Length);
		execution.PayCycleEnergy(a);
	}
}