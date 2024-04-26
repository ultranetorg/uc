using System.Linq;

namespace Uccs.Net
{
	public class CostRequest : RdcCall<CostResponse>
	{
		public Transaction[]	Transactions {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
	
				return new CostResponse{RentPerBytePerDay = sun.Mcv.LastConfirmedRound.RentPerBytePerDay,
										ConsensusExeunitFee = sun.Mcv.LastConfirmedRound.ConsensusExeunitFee};
			}
		}
	}
	
	public class CostResponse : RdcResponse
	{
		public Money RentPerBytePerDay { get; set; }
		public Money ConsensusExeunitFee { get; set; }
	}
}

