namespace Uccs.Rdn
{
	public class FileInfoRequest : RdnPpc<FileInfoResponse>
	{
		public Urr		Release { get; set; }
		public string	File { get; set; }

		public override PeerResponse Execute()
		{
			lock(Node.ResourceHub.Lock)
			{
				if(Node.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);
				
				var r = Node.ResourceHub.Find(Release);
				
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
