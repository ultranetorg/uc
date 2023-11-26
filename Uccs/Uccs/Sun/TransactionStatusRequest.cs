using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class TransactionStatusRequest : RdcRequest
	{
		public TransactionsAddress[]	Transactions { get; set; }

		protected override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
	
				return new TransactionStatusResponse
						{
							LastConfirmedRoundId = sun.Mcv.LastConfirmedRound.Id,
							Transactions = Transactions.Select(t => new{Q = t,
																		T = sun.IncomingTransactions.Find(i => i.Signer == t.Account && i.Nid == t.Nid)
																			?? 
																			sun.Mcv.Accounts.FindLastTransaction(t.Account, i => i.Nid == t.Nid)})
														.Select(i => new TransactionStatusResponse.Item{Account		= i.Q.Account,
																										Nid			= i.Q.Nid,
																										Placing		= i.T == null ? PlacingStage.FailedOrNotFound : i.T.Placing})
														.ToArray()
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

		public int		LastConfirmedRoundId { get; set; }
		public Item[]	Transactions { get; set; }
	}
}
