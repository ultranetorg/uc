using System.Net;

namespace Uccs.Rdn;

public class LocateReleasePpc : RdnPpc<LocateReleasePpr>
{
	public Urr	Address { get; set; }
	public int	Count { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new LocateReleasePpr {Seeders = Node.SeedHub.Locate(this)}; 
	}
}
	
public class LocateReleasePpr : Result
{
	public Endpoint[]	Seeders { get; set; }
}
