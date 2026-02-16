namespace Uccs.Fair;

public class ProductPpc : FairPpc<ProductPpr>
{
	public new AutoId	Id { get; set; }

	public ProductPpc()
	{
	}

	public ProductPpc(AutoId identifier)
	{
		Id = identifier;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{	
			var	r = Mcv.Products.Latest(Id);
							
			if(r == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProductPpr {Product = r};
		}
	}
}
	
public class ProductPpr : Result
{
	public Product Product { get; set; }
}
