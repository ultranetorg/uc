namespace Uccs.Net
{
	public class FileInfoRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }

		protected override RdcResponse Execute(Sun sun)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(sun.ResourceHub == null) 
					throw new RdcNodeException(RdcNodeError.NotSeed);
				
				var r = sun.ResourceHub.Find(Resource, Hash);
				
				if(r == null || !r.IsReady(File)) 
					throw new RdcEntityException(RdcEntityError.NotFound);
	
				return new FileInfoResponse{Length = r.GetLength(File)};
			}
		}
	}

	public class FileInfoResponse : RdcResponse
	{
		public long Length { get; set; }
	}
}
