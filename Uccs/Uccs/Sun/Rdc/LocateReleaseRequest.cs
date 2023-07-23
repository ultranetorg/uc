using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdcRequest
	{
		public ResourceAddress	Release { get; set; }
		public int				Count { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Seedbase == null) throw new RdcNodeException(RdcNodeError.NotHub);

			return new LocateReleaseResponse {Seeders = core.Seedbase.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}
}
