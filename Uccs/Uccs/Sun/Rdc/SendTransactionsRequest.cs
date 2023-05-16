using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class SendTransactionsRequest : RdcRequest
	{
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new  RdcNodeException(RdcNodeError.NotSynchronized);
				else
				{
					var acc = core.ProcessIncoming(Transactions);

					return new SendTransactionsResponse {Accepted = acc.Select(i => i.Signature).ToList()};
				}
		}
	}
	
	public class SendTransactionsResponse : RdcResponse
	{
		public IEnumerable<byte[]> Accepted { get; set; }
	}
}

