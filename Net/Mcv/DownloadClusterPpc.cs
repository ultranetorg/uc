namespace Uccs.Net;

public class DownloadClusterPpc : McvPpc<DownloadClusterPpr>
{
	public int		Table { get; set; }
	public byte[]	Hash { get; set; }
	public short	Cluster { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			if(Mcv.Tables[Table].IsIndex)
				throw new RequestException(RequestError.IncorrectRequest);

			var c = Mcv.Tables[Table].FindCluster(Cluster);

			if(c == null)
				throw new EntityException(EntityError.NotFound);

			if(!c.Hash.SequenceEqual(Hash))
				throw new EntityException(EntityError.HashMismatach);

			return new DownloadClusterPpr {Main = c.Export()};
		}
	}
}
	
public class DownloadClusterPpr : Result
{
	public byte[] Main { get; set; }
}
