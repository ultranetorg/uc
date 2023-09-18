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
			if(sun.ResourceHub == null) 
				throw new RdcNodeException(RdcNodeError.NotSeeder);
			
			if(!sun.ResourceHub.Exists(Resource, Hash, File)) 
				throw new RdcNodeException(RdcNodeError.NotFound);

			return new DownloadReleaseResponse{Data = sun.ResourceHub.ReadFile(Resource, Hash, File, Offset, Length)};
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
