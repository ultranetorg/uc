namespace Uccs.Net;

public class DownloadBucketPpc : McvPpc<DownloadBucketPpr>
{
	public int		Table { get; set; }
	public byte[]	Hash { get; set; }
	public int		Bucket { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			if(Mcv.Tables[Table].IsIndex)
				throw new RequestException(RequestError.IncorrectRequest);

			var b = Mcv.Tables[Table].FindBucket(Bucket);

			if(b == null)
				throw new EntityException(EntityError.NotFound);

			if(!b.Hash.SequenceEqual(Hash))
				throw new EntityException(EntityError.HashMismatach);

			return new DownloadBucketPpr {Main = b.Export()};
		}
	}
}
	
public class DownloadBucketPpr : Result
{
	public byte[] Main { get; set; }
}
