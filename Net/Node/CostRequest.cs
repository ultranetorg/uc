using System.Linq;

namespace Uccs.Net
{
	public class CostRequest : PeerCall<CostResponse>
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

