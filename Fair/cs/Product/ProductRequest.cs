namespace Uccs.Fair;

public class ProductRequest : FairPpc<ProductResponse>
{
	public new EntityId	Id { get; set; }

	public ProductRequest()
	{
	}

	public ProductRequest(EntityId identifier)
	{
		Id = identifier;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{	
			var	r = Mcv.Products.Find(Id, Mcv.LastConfirmedRound.Id);
							
			if(r == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProductResponse {Product = r};
		}
	}
}
	
public class ProductResponse : PeerResponse
{
	public Product Product { get; set; }
}
	
