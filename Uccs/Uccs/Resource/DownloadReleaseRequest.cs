namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdcRequest
	{
		public byte[]			Release { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(sun.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);

				var r = sun.ResourceHub.Find(Release);
				
				if(r == null || !r.IsReady(File)) 
					throw new EntityException(EntityError.NotFound);
	
				return new DownloadReleaseResponse {Data = r.ReadFile(File, Offset, Length)};
			}
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
