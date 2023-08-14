using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class SendTransactionsRequest : RdcRequest
	{
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
				if(sun.Synchronization != Synchronization.Synchronized)
					throw new  RdcNodeException(RdcNodeError.NotSynchronized);
				else
				{
					var acc = sun.ProcessIncoming(Transactions);

					return new SendTransactionsResponse {Accepted = acc.Select(i => i.Signature).ToList()};
				}
		}
	}
	
	public class SendTransactionsResponse : RdcResponse
	{
		public IEnumerable<byte[]> Accepted { get; set; }
	}
}

