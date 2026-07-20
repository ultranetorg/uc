namespace Uccs.Net;

public class FundsPpc : McvPpc<FundsPpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		return new FundsPpr {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
	}
}

public class FundsPpr : Result
{
	public PublicKey[] Funds { get; set; }
}
