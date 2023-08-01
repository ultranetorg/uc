using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdcRequest
	{
		public byte[]	Hash { get; set; }
		public int		Count { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(!core.IsMember) throw new RdcNodeException(RdcNodeError.NotMember);

			return new LocateReleaseResponse {Seeders = core.Seedbase.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}
}
