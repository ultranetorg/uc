namespace Uccs.Fair;

public class CatalogueTopicsRequest : FairPpc<CatalogueTopicsResponse>
{
	public EntityId		Catalogue {get; set;}

	public CatalogueTopicsRequest()
	{
	}

	public CatalogueTopicsRequest(EntityId id)
	{
		Catalogue = id;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			var e = Mcv.Catalogues.Find(Catalogue, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CatalogueTopicsResponse {Topics = e.Topics};
		}
	}
}

public class CatalogueTopicsResponse : PeerResponse
{
	public EntityId[] Topics {get; set;}
}
