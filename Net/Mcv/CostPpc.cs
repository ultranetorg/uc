namespace Uccs.Net;

public class CostPpc : McvPpc<CostPpr>
{
	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			RequireGraph();

			return new CostPpr{//RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
									ConsensusExecutionFee = Mcv.LastConfirmedRound.ConsensusECEnergyCost};
		}
	}
}

public class CostPpr : PeerResponse
{
	//public Money RentPerBytePerDay { get; set; }
	public long ConsensusExecutionFee { get; set; }
}

