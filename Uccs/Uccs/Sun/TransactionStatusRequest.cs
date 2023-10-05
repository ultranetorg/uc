using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class TransactionStatusRequest : RdcRequest
	{
		public IEnumerable<TransactionsAddress>	Transactions { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(!sun.Roles.HasFlag(Role.Chain))
					throw new RdcNodeException(RdcNodeError.NotChain);

				if(sun.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
	
				return	new TransactionStatusResponse
						{
							LastConfirmedRoundId = sun.Mcv.LastConfirmedRound.Id,
							Transactions = Transactions.Select(t => new {	Q = t,
																			T = sun.IncomingTransactions.Find(i => i.Signer == t.Account && i.Nid == t.Nid)
																				?? 
																				sun.Mcv.Accounts.FindLastTransaction(t.Account, i => i.Nid == t.Nid)})
														.Select(i => new TransactionStatusResponse.Item{Account		= i.Q.Account,
																										Nid			= i.Q.Nid,
																										Placing		= i.T == null ? PlacingStage.FailedOrNotFound : i.T.Placing}).ToArray()
						};
			}
		}
	}
	
	public class TransactionStatusResponse : RdcResponse
	{
		public class Item
		{
			public AccountAddress	Account { get; set; }
			public int				Nid { get; set; }
			public PlacingStage		Placing { get; set; }
		}

		public int					LastConfirmedRoundId { get; set; }
		public IEnumerable<Item>	Transactions { get; set; }
	}
}
