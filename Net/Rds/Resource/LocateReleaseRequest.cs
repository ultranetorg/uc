using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdsCall<LocateReleaseResponse>
	{
		public Urr	Address { get; set; }
		public int	Count { get; set; }

		public override PeerResponse Execute()
		{
			lock(Rds.Lock)
			{	
				RequireMember();
			}

			lock(Rds.SeedHub.Lock)
				return new LocateReleaseResponse {Seeders = Rds.SeedHub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : PeerResponse
	{
		public IPAddress[]	Seeders { get; set; }
	}
}
