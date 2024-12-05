namespace Uccs.Net
{
	public class DownloadTableRequest : McvPpc<DownloadTableResponse>
	{
		public int		Table { get; set; }
		public byte[]	Hash { get; set; }
		public int		BucketId { get; set; }

		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();

				var b = Mcv.Tables[Table].FindBucket(BucketId);

				if(b == null)
					throw new EntityException(EntityError.NotFound);
	
				if(!b.Hash.SequenceEqual(Hash))
					throw new EntityException(EntityError.HashMismatach);

	
				return new DownloadTableResponse {Main = b.Main};
			}
		}
	}
		
	public class DownloadTableResponse : PeerResponse
	{
		public byte[] Main { get; set; }
	}
}
