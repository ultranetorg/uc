using Org.BouncyCastle.Asn1.Ocsp;

namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdsCall<DownloadReleaseResponse>
	{
		public Urr				Address { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override PeerResponse Execute()
		{
			if(Length > ResourceHub.PieceMaxLength)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Rds.ResourceHub.Lock)
			{
				if(Rds.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);

				var r = Rds.ResourceHub.Find(Address);
				
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
