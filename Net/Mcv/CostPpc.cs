namespace Uccs.Net;

public class CostPpc : McvPpc<CostPpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		lock(Mcv.Lock)
		{
			return new CostPpr{//RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
									ConsensusExecutionFee = Mcv.LastConfirmedRound.ConsensusEnergyCost};
		}
	}
}

public class CostPpr : Result
{
	//public Money RentPerBytePerDay { get; set; }
	public long ConsensusExecutionFee { get; set; }
}

