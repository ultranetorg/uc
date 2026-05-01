namespace Uccs.Fair;

public class ProductDeletion : FairOperation
{
	public AutoId				Product { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Product={Product}";

	public ProductDeletion()
	{
	}

	public override void Read(Reader reader)
	{
		Product = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Product);
	}

	public override void Execute(FairExecution execution)
	{
		if(CanAccessProduct(execution, Product, out var a, out var p, out Error) == false)
			return;

		a = execution.Authors.Affect(p.Author);
		p = execution.Products.Affect(p.Id);
		p.Deleted = true;

		foreach(var i in p.Publications)
			execution.Publications.Delete(i);

		var d = Uccs.Fair.Product.FindDeclaration(p.Type);

		foreach(var i in p.Versions)
		{
			i.ForEach(d, (f, i) =>	{
										if(f.Type == FieldType.FileId)
										{	
											var x = execution.Files.Affect(i.AsAutoId);
											x.Refs--;
	 									}
									});
		}

		a.Products = a.Products.Remove(p.Id);

		execution.Free(a, a, execution.Net.EntityLength + p.Length);
		execution.PayOperationEnergy(a);
	}
}