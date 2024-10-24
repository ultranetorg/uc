﻿namespace Uccs.Rdn
{
	public class CostRequest : RdnPpc<CostResponse>
	{
		public Transaction[]	Transactions {get; set;}

		public override PeerResponse Execute()
		{
			lock(Peering.Lock)
			{
				RequireBase();
	
				return new CostResponse{//RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
										ConsensusExecutionFee = Mcv.LastConfirmedRound.ConsensusExecutionFee};
			}
		}
	}
	
	public class CostResponse : PeerResponse
	{
		//public Money RentPerBytePerDay { get; set; }
		public long ConsensusExecutionFee { get; set; }
	}
}

