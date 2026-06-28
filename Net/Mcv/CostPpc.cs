namespace Uccs.Net;

public class CostPpc : McvPpc<CostPpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		return new CostPpr{//RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
								ConsensusExecutionFee = Mcv.LastConfirmedRound.ConsensusOperationCost};
	}
}

public class CostPpr : Result
{
	//public Money RentPerBytePerDay { get; set; }
	public long ConsensusExecutionFee { get; set; }
}

