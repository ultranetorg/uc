namespace Uccs.Net;

public class FundsPpc : McvPpc<FundsPpr>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			return new FundsPpr {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
		}
	}
}

public class FundsPpr : PeerResponse
{
	public AccountAddress[] Funds { get; set; }
}
