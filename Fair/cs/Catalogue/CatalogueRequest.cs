namespace Uccs.Fair;

public class CatalogueRequest : FairPpc<CatalogueResponse>
{
	public new EntityId	Id { get; set; }

	public CatalogueRequest()
	{
	}

	public CatalogueRequest(EntityId id)
	{
		Id = id;
	}

	public override PeerResponse Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireBase();

			var	e = Mcv.Catalogues.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CatalogueResponse {Catalogue = e};
		}
	}
}

public class CatalogueResponse : PeerResponse
{
	public Catalogue	Catalogue {get; set;}
}
