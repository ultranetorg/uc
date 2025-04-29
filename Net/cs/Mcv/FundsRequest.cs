namespace Uccs.Net;

public class FundsRequest : McvPpc<FundsResponse>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			return new FundsResponse {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
		}
	}
}

public class FundsResponse : PeerResponse
{
	public AccountAddress[] Funds { get; set; }
}
