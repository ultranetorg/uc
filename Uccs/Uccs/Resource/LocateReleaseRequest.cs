using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdcRequest
	{
		public byte[]	Hash { get; set; }
		public int		Count { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(!sun.IsMember) throw new RdcNodeException(RdcNodeError.NotMember);

			lock(sun.SeedHub.Lock)
				return new LocateReleaseResponse {Seeders = sun.SeedHub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}
}
