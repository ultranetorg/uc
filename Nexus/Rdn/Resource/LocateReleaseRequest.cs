using System.Net;

namespace Uccs.Rdn;

public class LocateReleaseRequest : RdnPpc<LocateReleaseResponse>
{
	public Urr	Address { get; set; }
	public int	Count { get; set; }

	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new LocateReleaseResponse {Seeders = Node.SeedHub.Locate(this)}; 
	}
}
	
public class LocateReleaseResponse : PeerResponse
{
	public IPAddress[]	Seeders { get; set; }
}
