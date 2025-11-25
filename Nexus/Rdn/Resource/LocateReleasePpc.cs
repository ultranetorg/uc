using System.Net;

namespace Uccs.Rdn;

public class LocateReleasePpc : RdnPpc<LocateReleasePpr>
{
	public Urr	Address { get; set; }
	public int	Count { get; set; }

	public override Return Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new LocateReleasePpr {Seeders = Node.SeedHub.Locate(this)}; 
	}
}
	
public class LocateReleasePpr : Return
{
	public IPAddress[]	Seeders { get; set; }
}
