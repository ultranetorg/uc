namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(sun.ResourceHub == null) 
					throw new RdcNodeException(RdcNodeError.NotSeed);

				var r = sun.ResourceHub.Find(Resource, Hash);
				
				if(r == null || !r.IsReady(File)) 
					throw new RdcEntityException(RdcEntityError.NotFound);
	
				return new DownloadReleaseResponse {Data = r.ReadFile(File, Offset, Length)};
			}
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
