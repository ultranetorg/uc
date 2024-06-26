using System.Linq;

namespace Uccs.Rdn
{
	public class CostRequest : RdnCall<CostResponse>
	{
		public Transaction[]	Transactions {get; set;}

		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
	
				return new CostResponse{RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
										ConsensusExeunitFee = Mcv.LastConfirmedRound.ConsensusExeunitFee};
			}
		}
	}
	
	public class CostResponse : PeerResponse
	{
		public Money RentPerBytePerDay { get; set; }
		public Money ConsensusExeunitFee { get; set; }
	}
}

