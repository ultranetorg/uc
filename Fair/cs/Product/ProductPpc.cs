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

	public override Return Execute()
	{
 		lock(Mcv.Lock)
		{	
			var	r = Mcv.Products.Find(Id, Mcv.LastConfirmedRound.Id);
							
			if(r == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProductPpr {Product = r};
		}
	}
}
	
public class ProductPpr : Return
{
	public Product Product { get; set; }
}
