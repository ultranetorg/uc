using System.Linq;

namespace Uccs.Net
{
	public class PlaceTransactionsRequest : PeerCall<PlaceTransactionsResponse>
	{
		public Transaction[]	Transactions {get; set;}

		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireMember();
	
				var acc = Mcv.ProcessIncoming(Transactions).Select(i => i.Signature).ToArray();

				return new PlaceTransactionsResponse {Accepted = acc};
			}
		}
	}
	
	public class PlaceTransactionsResponse : PeerResponse
	{
		public byte[][] Accepted { get; set; }
	}
}

