using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class PlaceTransactionsRequest : RdcRequest
	{
		public Transaction[]	Transactions {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireMember(sun);
	
				var acc = sun.ProcessIncoming(Transactions);

				return new PlaceTransactionsResponse {Accepted = acc.Select(i => i.Signature).ToArray()};
			}
		}
	}
	
	public class PlaceTransactionsResponse : RdcResponse
	{
		public byte[][] Accepted { get; set; }
	}
}

