namespace Uccs.Net;

public class FundsPpc : McvPpc<FundsPpr>
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			return new FundsPpr {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
		}
	}
}

public class FundsPpr : Result
{
	public AccountAddress[] Funds { get; set; }
}
