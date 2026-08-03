namespace Uccs.Fair;

public class ProductPpc : FairPpc<ProductPpr>
{
	public AutoId	Id { get; set; }

	public ProductPpc()
	{
	}

	public ProductPpc(AutoId identifier)
	{
		Id = identifier;
	}

	public override Result Execute()
	{
		var	r = Mcv.Products.Latest(Id);
							
		if(r == null)
			throw new EntityException(EntityError.NotFound);
			
		return new ProductPpr {Product = r};
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}
}
	
public class ProductPpr : Result
{
	public Product Product { get; set; }

	public override void Read(Reader reader)
	{
		Product = reader.Read<Product>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Product);
	}
}
