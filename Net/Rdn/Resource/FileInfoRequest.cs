namespace Uccs.Net
{
	public class FileInfoRequest : RdnCall<FileInfoResponse>
	{
		public Urr		Release { get; set; }
		public string	File { get; set; }

		public override PeerResponse Execute()
		{
			lock(Rdn.ResourceHub.Lock)
			{
				if(Rdn.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);
				
				var r = Rdn.ResourceHub.Find(Release);
				
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
