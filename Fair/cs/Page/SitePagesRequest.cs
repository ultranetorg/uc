namespace Uccs.Fair;

public class SitePagesRequest : FairPpc<SitePagesResponse>
{
	public EntityId		Site {get; set;}

	public SitePagesRequest()
	{
	}

	public SitePagesRequest(EntityId id)
	{
		Site = id;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			var e = Mcv.Sites.Find(Site, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SitePagesResponse {Pages = e.Roots};
		}
	}
}

public class SitePagesResponse : PeerResponse
{
	public EntityId[] Pages {get; set;}
}
