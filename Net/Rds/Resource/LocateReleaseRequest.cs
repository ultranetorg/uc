using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdcCall<LocateReleaseResponse>
	{
		public Urr	Address { get; set; }
		public int				Count { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{	
				RequireRdsBase(sun);
				RequireMember(sun);
			}

			lock(sun.SeedHub.Lock)
				return new LocateReleaseResponse {Seeders = sun.SeedHub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IPAddress[]	Seeders { get; set; }
	}
}
