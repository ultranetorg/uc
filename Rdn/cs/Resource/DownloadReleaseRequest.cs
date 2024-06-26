namespace Uccs.Rdn
{
	public class DownloadReleaseRequest : RdnCall<DownloadReleaseResponse>
	{
		public Urr				Address { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override PeerResponse Execute()
		{
			if(Length > ResourceHub.PieceMaxLength)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Node.ResourceHub.Lock)
			{
				if(Node.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);

				var r = Node.ResourceHub.Find(Address);
				
				if(r == null || !r.IsReady(File)) 
					throw new EntityException(EntityError.NotFound);
	
				return new DownloadReleaseResponse {Data = r.Find(File).Read(Offset, Length)};
			}
		}
	}

	public class DownloadReleaseResponse : PeerResponse
	{
		public byte[] Data { get; set; }
	}
}
