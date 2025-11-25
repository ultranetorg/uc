namespace Uccs.Net;

public class DownloadTablePpc : McvPpc<DownloadTablePpr>
{
	public int		Table { get; set; }
	public byte[]	Hash { get; set; }
	public int		BucketId { get; set; }

	public override Return Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			if(Mcv.Tables[Table].IsIndex)
				throw new RequestException(RequestError.IncorrectRequest);

			var b = Mcv.Tables[Table].FindBucket(BucketId);

			if(b == null)
				throw new EntityException(EntityError.NotFound);

			if(!b.Hash.SequenceEqual(Hash))
				throw new EntityException(EntityError.HashMismatach);

			return new DownloadTablePpr {Main = b.Export()};
		}
	}
}
	
public class DownloadTablePpr : Return
{
	public byte[] Main { get; set; }
}
