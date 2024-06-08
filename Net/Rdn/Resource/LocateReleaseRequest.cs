using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class LocateReleaseRequest : RdnCall<LocateReleaseResponse>
	{
		public Urr	Address { get; set; }
		public int	Count { get; set; }

		public override PeerResponse Execute()
		{
			lock(Rdn.Lock)
			{	
				RequireMember();
			}

			lock(Rdn.SeedHub.Lock)
				return new LocateReleaseResponse {Seeders = Rdn.SeedHub.Locate(this)}; 
		}
	}
		
	public class LocateReleaseResponse : PeerResponse
	{
		public IPAddress[]	Seeders { get; set; }
	}
}
