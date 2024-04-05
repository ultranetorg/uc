namespace Uccs.Net
{
	public class FileInfoRequest : RdcCall<FileInfoResponse>
	{
		public ReleaseAddress	Release { get; set; }
		public string			File { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(sun.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);
				
				var r = sun.ResourceHub.Find(Release);
				
				if(r == null || !r.IsReady(File)) 
					throw new EntityException(EntityError.NotFound);
	
				return new FileInfoResponse{Length = r.GetLength(File)};
			}
		}
	}

	public class FileInfoResponse : RdcResponse
	{
		public long Length { get; set; }
	}
}
