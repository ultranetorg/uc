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

			lock(sun.Hub.Lock)
				return new LocateReleaseResponse {Seeders = sun.Hub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}
}
