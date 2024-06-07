namespace Uccs.Net
{
	public class FileInfoRequest : RdsCall<FileInfoResponse>
	{
		public Urr		Release { get; set; }
		public string	File { get; set; }

		public override PeerResponse Execute()
		{
			lock(Rds.ResourceHub.Lock)
			{
				if(Rds.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);
				
				var r = Rds.ResourceHub.Find(Release);
				
				if(r == null || !r.IsReady(File)) 
					throw new EntityException(EntityError.NotFound);
	
				return new FileInfoResponse{Length = r.Find(File).Length};
			}
		}
	}

	public class FileInfoResponse : PeerResponse
	{
		public long Length { get; set; }
	}
}
