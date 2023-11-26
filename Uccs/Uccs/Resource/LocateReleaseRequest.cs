using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdcRequest
	{
		public byte[]	Hash { get; set; }
		public int		Count { get; set; }

		protected override RdcResponse Execute(Sun sun)
		{
			RequireMember(sun);

			lock(sun.SeedHub.Lock)
				return new LocateReleaseResponse {Seeders = sun.SeedHub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : RdcResponse
	{
		public IPAddress[]	Seeders { get; set; }
	}
}
