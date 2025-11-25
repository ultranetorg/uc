namespace Uccs.Net;

public class FundsPpc : McvPpc<FundsPpr>
{
	public override Return Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			return new FundsPpr {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
		}
	}
}

public class FundsPpr : Return
{
	public AccountAddress[] Funds { get; set; }
}
