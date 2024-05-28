using Org.BouncyCastle.Asn1.Ocsp;

namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdcCall<DownloadReleaseResponse>
	{
		public Urr	Address { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(Length > ResourceHub.PieceMaxLength)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(sun.ResourceHub.Lock)
			{
				if(sun.ResourceHub == null) 
					throw new NodeException(NodeError.NotSeed);

				var r = sun.ResourceHub.Find(Address);
				
				if(r == null || !r.IsReady(File)) 
					throw new EntityException(EntityError.NotFound);
	
				return new DownloadReleaseResponse {Data = r.Find(File).Read(Offset, Length)};
			}
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
