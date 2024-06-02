using System.Linq;

namespace Uccs.Net
{
	public class PlaceTransactionsRequest : RdcCall<PlaceTransactionsResponse>
	{
		public Transaction[]	Transactions {get; set;}

		public override RdcResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireMember();
	
				var acc = Mcv.ProcessIncoming(Transactions).Select(i => i.Signature).ToArray();

				return new PlaceTransactionsResponse {Accepted = acc};
			}
		}
	}
	
	public class PlaceTransactionsResponse : RdcResponse
	{
		public byte[][] Accepted { get; set; }
	}
}

