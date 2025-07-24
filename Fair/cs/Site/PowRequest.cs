namespace Uccs.Fair;

public class PowRequest : FairPpc<PowResponse>
{
	public AutoId	Site { get; set; }

	public PowRequest()
	{
	}

	public PowRequest(AutoId id)
	{
		Site = id;
	}

	public override PeerResponse Execute()
	{
		if(Site == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Sites.Latest(Site);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new PowResponse {Complexity = e.PoWComplexity, GraphHash = Mcv.GraphHash};
		}
	}
}

public class PowResponse : PeerResponse
{
	public int		Complexity {get; set;}
	public byte[]	GraphHash {get; set;}
}
