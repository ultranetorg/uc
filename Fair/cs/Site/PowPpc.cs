namespace Uccs.Fair;

public class PowPpc : FairPpc<PowPpr>
{
	public AutoId	Site { get; set; }

	public PowPpc()
	{
	}

	public PowPpc(AutoId id)
	{
		Site = id;
	}

	public override Return Execute()
	{
		if(Site == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Sites.Latest(Site);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new PowPpr {Complexity = e.PoWComplexity, GraphHash = Mcv.GraphHash};
		}
	}
}

public class PowPpr : Return
{
	public int		Complexity {get; set;}
	public byte[]	GraphHash {get; set;}
}
